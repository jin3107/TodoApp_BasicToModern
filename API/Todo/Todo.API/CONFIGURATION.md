# Configuration Setup Guide

## Security Notice
Sensitive configuration values are **NOT** stored in `appsettings.json` for security reasons.

## Setup Instructions

### Option 1: User Secrets (Recommended for Development)

Run these commands in the `Todo.API` directory:

```bash
# Initialize user secrets (already done)
dotnet user-secrets init

# Set database connection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=Todo;User=YourUser;Password=YourPassword;"

# Set email settings
dotnet user-secrets set "EmailSettings:SmtpUsername" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPassword" "your-app-password"
dotnet user-secrets set "EmailSettings:FromEmail" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:RecipientEmail" "recipient@example.com"
```

### Option 2: appsettings.Development.json (Local Development)

Create or edit `appsettings.Development.json` with your actual values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Todo;User=root;Password=YourPassword;"
  },
  "EmailSettings": {
    "SmtpUsername": "your-email@gmail.com",
 "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "RecipientEmail": "recipient@example.com"
  }
}
```

**?? Note:** `appsettings.Development.json` is in `.gitignore` and will NOT be committed.

### Option 3: Environment Variables (Production)

Set these environment variables on your server:

```bash
# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="Server=localhost;..."
$env:EmailSettings__SmtpPassword="your-password"

# Linux/Mac
export ConnectionStrings__DefaultConnection="Server=localhost;..."
export EmailSettings__SmtpPassword="your-password"
```

## Gmail App Password Setup

To get an app password for Gmail:
1. Enable 2-Factor Authentication on your Google Account
2. Go to: https://myaccount.google.com/apppasswords
3. Generate a new app password
4. Use that password in your configuration

## Verify Your Configuration

```bash
cd Todo.API
dotnet user-secrets list
```

This will show all your stored secrets without exposing them in source control.
