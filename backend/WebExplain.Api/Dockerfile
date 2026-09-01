FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY WebExplain.Api.csproj .
RUN dotnet restore WebExplain.Api.csproj
COPY . .
RUN dotnet publish WebExplain.Api.csproj -c Release -o /app/publish --no-restore

# Using the official aspnet:9.0 image (rather than mcr.microsoft.com/playwright/dotnet,
# whose .NET runtime version drifts independently of this project's target framework and
# previously shipped .NET 10 against our net9.0 build) keeps the runtime version pinned to
# what we actually build against. PowerShell is only needed transiently to run the
# Playwright-generated install script; Chromium and its OS dependencies are what remain.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

RUN apt-get update \
    && apt-get install -y --no-install-recommends wget ca-certificates gnupg apt-transport-https \
    && wget -q https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb \
    && dpkg -i /tmp/packages-microsoft-prod.deb \
    && rm /tmp/packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y --no-install-recommends powershell \
    && pwsh playwright.ps1 install --with-deps chromium \
    && apt-get purge -y --auto-remove powershell wget gnupg apt-transport-https \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "WebExplain.Api.dll"]
