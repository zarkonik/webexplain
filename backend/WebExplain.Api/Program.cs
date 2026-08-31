using Microsoft.EntityFrameworkCore;
using WebExplain.Api.Data;
using WebExplain.Api.Repositories;
using WebExplain.Api.Services;
using WebExplain.Api.Services.Capture;
using WebExplain.Api.Services.LiveCapture;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();
