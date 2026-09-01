using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebExplain.Api.Data;
using WebExplain.Api.Repositories;
using WebExplain.Api.Services;
using WebExplain.Api.Services.Capture;
using WebExplain.Api.Services.LiveCapture;

// Without this, ASP.NET Core silently renames well-known JWT claims on the way in (e.g.
// "sub" becomes ClaimTypes.NameIdentifier), so code that reads JwtRegisteredClaimNames.Sub
// from the authenticated ClaimsPrincipal would never find it.
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    // Every endpoint requires a valid JWT by default; AuthController's register/login
    // actions opt out explicitly with [AllowAnonymous].
    options.Filters.Add(new AuthorizeFilter());
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSection["Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        // <img>/<a> tags can't send an Authorization header, so screenshot and export
        // endpoints accept the token as a query parameter too. Everything else still
        // requires the standard header - this only widens how the token can arrive.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IGuideRepository, GuideRepository>();
builder.Services.AddScoped<IGuideService, GuideService>();
builder.Services.AddScoped<IGuideExportService, GuideExportService>();

builder.Services.AddScoped<ICaptureSessionRepository, CaptureSessionRepository>();
builder.Services.AddScoped<ICaptureService, CaptureService>();
builder.Services.AddSingleton<IBrowserCaptureEngine, PlaywrightCaptureEngine>();

builder.Services.AddSingleton<ILiveCaptureManager, LiveCaptureManager>();
builder.Services.AddHostedService<LiveCaptureCleanupService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Applies any pending EF Core migrations on startup, since the production container has
// no dotnet-ef tool available to run them as a separate step.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// In production, Caddy terminates HTTPS at the edge and forwards plain HTTP to this
// container over the internal Docker network - redirecting here too would just be
// redundant (and Kestrel doesn't see TLS, so it can't tell the request was already secure).
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
