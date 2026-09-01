FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY WebExplain.Api.csproj .
RUN dotnet restore WebExplain.Api.csproj
COPY . .
RUN dotnet publish WebExplain.Api.csproj -c Release -o /app/publish --no-restore

# The Playwright team's own runtime image already has Chromium plus every OS-level
# library it needs, pinned to a specific Playwright version - keep this tag in sync with
# the Microsoft.Playwright package version in WebExplain.Api.csproj (currently 1.62.0).
FROM mcr.microsoft.com/playwright/dotnet:v1.62.0-noble AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "WebExplain.Api.dll"]
