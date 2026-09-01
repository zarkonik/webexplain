# Deployment scripts

## install-playwright-ubuntu.sh

Installs Chromium plus all the OS-level libraries Playwright needs to run headless
(fonts, GTK/NSS libs, etc.) on a fresh Ubuntu VPS. Also installs PowerShell if it's
missing, since the Microsoft.Playwright NuGet package only ships a `playwright.ps1`
install script, even on Linux.

Run once per server, after publishing the backend:

```bash
dotnet publish backend/WebExplain.Api -c Release -o /var/www/webexplain/backend
./deploy/install-playwright-ubuntu.sh /var/www/webexplain/backend
```

Re-run it after any Microsoft.Playwright package version bump, since the installed
Chromium build is tied to that version.
