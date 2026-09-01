#!/usr/bin/env bash
# Installs everything the backend's Playwright/Chromium capture needs on a fresh Ubuntu VPS.
#
# Why PowerShell: the Microsoft.Playwright NuGet package only ships a playwright.ps1
# install script (even for Linux builds), so pwsh has to be present to run it.
#
# Run this once per VPS, after `dotnet publish` has produced the backend output folder.
# Usage: ./install-playwright-ubuntu.sh /path/to/backend/publish/output

set -euo pipefail

PUBLISH_DIR="${1:?Usage: $0 <path-to-backend-publish-output>}"

if ! command -v pwsh >/dev/null 2>&1; then
  echo "Installing PowerShell..."
  sudo apt-get update
  sudo apt-get install -y wget apt-transport-https software-properties-common
  wget -q "https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb" -O /tmp/packages-microsoft-prod.deb
  sudo dpkg -i /tmp/packages-microsoft-prod.deb
  rm /tmp/packages-microsoft-prod.deb
  sudo apt-get update
  sudo apt-get install -y powershell
fi

PLAYWRIGHT_SCRIPT="$PUBLISH_DIR/playwright.ps1"
if [ ! -f "$PLAYWRIGHT_SCRIPT" ]; then
  echo "playwright.ps1 not found at $PLAYWRIGHT_SCRIPT" >&2
  echo "Make sure you passed the backend's publish/build output directory." >&2
  exit 1
fi

echo "Installing Chromium and its OS dependencies..."
pwsh "$PLAYWRIGHT_SCRIPT" install --with-deps chromium

echo "Done. Chromium is installed and ready for headless capture."
