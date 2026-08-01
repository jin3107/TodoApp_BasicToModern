# TodoApp Basic To Modern

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61DAFB?style=flat-square&logo=react&logoColor=111111)](https://react.dev/)
[![Vite](https://img.shields.io/badge/Vite-7-646CFF?style=flat-square&logo=vite&logoColor=ffffff)](https://vite.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?style=flat-square&logo=typescript&logoColor=ffffff)](https://www.typescriptlang.org/)
[![MySQL](https://img.shields.io/badge/MySQL-8-4479A1?style=flat-square&logo=mysql&logoColor=ffffff)](https://www.mysql.com/)
[![Redis](https://img.shields.io/badge/Redis-cache-DC382D?style=flat-square&logo=redis&logoColor=ffffff)](https://redis.io/)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE.txt)

A full-stack Todo application that starts with familiar CRUD workflows and grows into a more production-oriented system: cookie-based authentication, OTP email verification, dashboard reporting, Redis caching, Quartz background jobs, Docker deployment, and a modern React interface.

> This is a personal learning project. It is public for reference, but it is not currently maintained as a community contribution project.

## Table Of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Docker](#docker)
- [API Overview](#api-overview)
- [Troubleshooting](#troubleshooting)
- [What's Changed](#whats-changed)
- [Security Notes](#security-notes)
- [License](#license)
- [Author](#author)

## Features

- Todo list CRUD with search, pagination, and progress counts.
- Todo item CRUD with priority, due date, completion status, and filtering.
- Dashboard analytics with completion rate, overdue tasks, trends, and priority distribution.
- Cookie-based authentication using `AuthToken` and `RefreshToken` HttpOnly cookies.
- Email OTP flows for account verification and password changes.
- Fixed OTP verification flow and optimized post-login redirect to the dashboard.
- Refresh-token rotation and logout support.
- Role-based authorization for admin-only reports/jobs.
- Redis-backed caching for expensive report data, with memory-cache fallback.
- Quartz.NET background jobs for reports, reminders, summaries, and cleanup.
- Lazy-loaded React routes with authenticated-route preloading for a smoother login experience.
- Responsive Ant Design UI for mobile, tablet, laptop, and desktop.
- Polished Login/Register pages with auth illustrations, compact registration form layout, and route preload hints.
- Shared SCSS tokens, mixins, and layout utilities for cleaner frontend styling.
- Docker-ready backend, frontend, MySQL, Redis, and Nginx setup.

## Tech Stack

**Frontend**

- React 19
- TypeScript
- Vite
- React Router
- Ant Design
- Axios
- Day.js
- SCSS

**Backend**

- .NET 8
- ASP.NET Core Web API
- ASP.NET Core Identity
- Entity Framework Core
- MySQL
- Redis / Distributed Cache
- Quartz.NET
- MailKit
- Serilog

**Infrastructure**

- Docker Compose
- Nginx reverse proxy
- MySQL 8
- Redis 7

## Architecture

The backend follows Clean Architecture:

- `Todo.Domain`: entities, enums, value objects. No dependencies on any other project.
- `Todo.Application`: use-case handlers (auth, todos, reports, background jobs) and repository interfaces (`Interfaces/Repositories/`). Depends only on `Todo.Domain` and `Todo.DTOs`.
- `Todo.Infrastructure`: EF Core persistence (`Persistence/`: DbContext, entity configurations, migrations, repository implementations), Identity, Redis cache, Quartz jobs, and email. Implements the interfaces defined in `Todo.Application`.
- `Todo.API`: controllers, composition root (`Program.cs`), middleware configuration, CORS, auth, Swagger.
- `Todo.DTOs`: request and response contracts.
- `Todo.Commons`: shared enums and helpers.
- `MayNghien.Infrastructures`: shared infrastructure helpers (base entity/context, response wrappers, search helpers).

Request flow: Controllers (`Todo.API`) → Handlers (`Todo.Application`) → Repositories (`Todo.Infrastructure`). Repository interfaces stay domain-focused — no `DbSet<T>` or EF Core types leak into `Todo.Application`.

The frontend is feature-oriented:

- `src/routes`: lazy route definitions.
- `src/routes/preload.ts`: preloads authenticated route chunks from auth pages.
- `src/pages`: route-level pages.
- `src/apis`: API wrappers.
- `src/components`: reusable UI components.
- `src/commons`: shared frontend utilities and enums.
- `src/interfaces`: typed request/response contracts.
- `src/layouts`: shared layout shells.

## Project Structure

```text
TodoApp_BasicToModern/
├── TodoApp.Client/
│   ├── src/
│   │   ├── apis/
│   │   ├── commons/
│   │   ├── components/
│   │   ├── configs/
│   │   ├── interfaces/
│   │   ├── layouts/
│   │   ├── pages/
│   │   └── routes/
│   ├── Dockerfile
│   ├── package.json
│   └── vite.config.ts
├── TodoApp.Server/
│   └── src/
│       ├── MayNghien.Infrastructures/
│       ├── Todo.API/
│       ├── Todo.Application/
│       ├── Todo.Commons/
│       ├── Todo.Domain/
│       ├── Todo.DTOs/
│       ├── Todo.Infrastructure/
│       └── src.sln
├── nginx/
├── .env.example
├── docker-compose.yml.example
├── LICENSE.txt
└── README.md
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 18 or newer
- MySQL 8
- Redis 7, optional but recommended for report caching
- Docker Desktop, optional

### Backend

From the repository root:

```powershell
cd TodoApp.Server/src
dotnet restore
```

Configure local secrets from `Todo.API`:

```powershell
cd Todo.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=TodoApp_BToM;User=root;Password=your_password;"
dotnet user-secrets set "Jwt:Key" "replace-with-a-long-random-secret"
dotnet user-secrets set "Jwt:Issuer" "https://localhost:7196"
dotnet user-secrets set "Jwt:Audience" "https://localhost:7196"
dotnet user-secrets set "Bootstrap:AdminEmail" "your-admin-email@example.com"
dotnet user-secrets set "Bootstrap:AdminPassword" "your-secure-admin-password"
```

Optional Redis cache for development:

```powershell
dotnet user-secrets set "RedisSettings:Enabled" "true"
dotnet user-secrets set "ConnectionStrings:RedisConnection" "localhost:6379,abortConnect=false,connectTimeout=5000,syncTimeout=5000,asyncTimeout=5000,connectRetry=3,keepAlive=60"
```

Run migrations:

```powershell
cd ..
dotnet ef database update --project Todo.Infrastructure --startup-project Todo.API
```

Start the API:

```powershell
dotnet run --project Todo.API
```

Default local API URLs:

- HTTP API: `http://localhost:5133`
- HTTPS API: `https://localhost:7196`
- Swagger: `https://localhost:7196/swagger`

### Frontend

```powershell
cd TodoApp.Client
npm install
npm run dev
```

The client defaults to relative API routes. For direct backend calls, create a local `.env` in `TodoApp.Client` if needed:

```text
VITE_API_BASE_URL=https://localhost:7196
```

During Vite development, `vite.config.ts` proxies these relative routes to `http://localhost:5133`:

- `/authentication`
- `/todo-items`
- `/todo-lists`
- `/reports`
- `/jobs`

## Configuration

Keep real secrets out of Git.

Use these files as templates only:

- `.env.example`
- `docker-compose.yml.example`
- `nginx/conf.d/todoapp.conf.example`

Create local files when needed:

```powershell
Copy-Item .env.example .env
Copy-Item docker-compose.yml.example docker-compose.yml
Copy-Item nginx/conf.d/todoapp.conf.example nginx/conf.d/todoapp.conf
```

Recommended local secret storage:

- Backend development: `dotnet user-secrets`
- Docker deployment: `.env`
- Production server: environment variables or a secret manager

> `.env` is only read by `docker-compose`. Running the API directly with `dotnet run` never loads it — use `dotnet user-secrets` for local runs, including `EmailSettings:*` (see below).

### Bootstrap Admin

On first login with the email set in `Bootstrap:AdminEmail`, the app auto-creates a `SuperAdmin` account using `Bootstrap:AdminPassword`. Neither value ships in `appsettings.json` or Git — set both via `dotnet user-secrets` locally or `BOOTSTRAP_ADMIN_EMAIL`/`BOOTSTRAP_ADMIN_PASSWORD` in `.env` for Docker. If `Bootstrap:AdminEmail` is unset, bootstrap admin creation is disabled entirely.

### Gmail SMTP

Gmail requires an App Password, not your normal Gmail password.

```powershell
dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPort" "587"
dotnet user-secrets set "EmailSettings:SmtpUsername" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:SmtpPassword" "your-gmail-app-password"
dotnet user-secrets set "EmailSettings:FromEmail" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:FromName" "TodoApp"
dotnet user-secrets set "EmailSettings:RecipientEmail" "recipient@example.com"
```

## Docker

Copy examples first:

```powershell
Copy-Item .env.example .env
Copy-Item docker-compose.yml.example docker-compose.yml
```

Edit `.env`, then start services:

```powershell
docker compose up -d
```

For local development with only Redis:

```powershell
docker run -d --name todoapp-redis -p 127.0.0.1:6379:6379 redis:7-alpine redis-server --appendonly yes
```

Useful commands:

```powershell
docker compose ps
docker compose logs -f backend
docker compose logs -f redis
docker compose down
```

## API Overview

### Authentication

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/authentication/login` | Login and set auth cookies |
| `POST` | `/authentication/register` | Register a user and send verification OTP |
| `POST` | `/authentication/send-otp` | Send OTP for email verification or password change |
| `POST` | `/authentication/verify-otp` | Verify OTP |
| `POST` | `/authentication/change-password` | Change password after OTP verification |
| `POST` | `/authentication/refresh-token` | Refresh auth cookies |
| `POST` | `/authentication/logout` | Revoke session and clear cookies |

### Todo Lists

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/todo-lists/search` | Search todo lists |
| `GET` | `/todo-lists/{id}` | Get list by id |
| `POST` | `/todo-lists` | Create list |
| `PUT` | `/todo-lists` | Update list |
| `DELETE` | `/todo-lists/{id}` | Delete list |

### Todo Items

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/todo-items/search` | Search todo items |
| `GET` | `/todo-items/{id}` | Get item by id |
| `POST` | `/todo-items` | Create item |
| `PUT` | `/todo-items` | Update item |
| `DELETE` | `/todo-items/{id}` | Delete item |

### Reports And Jobs

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/reports/progress` | Get dashboard progress report |
| `POST` | `/reports/snapshot` | Create daily snapshot, admin only |
| `POST` | `/jobs/trigger/daily-report` | Trigger daily report, admin only |
| `POST` | `/jobs/trigger/weekly-summary` | Trigger weekly summary, admin only |
| `POST` | `/jobs/trigger/task-reminder` | Trigger reminder job, admin only |
| `POST` | `/jobs/pause/{jobName}` | Pause a Quartz job, admin only |
| `POST` | `/jobs/resume/{jobName}` | Resume a Quartz job, admin only |
| `GET` | `/jobs/scheduler/info` | Scheduler info, admin only |

## Troubleshooting

### Login succeeds but the app returns to `/login`

Check whether `RefreshToken` is stored and sent by the browser. In development, cookie settings depend on whether requests are made through HTTP proxy or direct HTTPS backend calls.

Also verify:

- `withCredentials: true` is enabled in Axios.
- Backend was restarted after cookie configuration changes.
- Old localhost cookies were cleared.
- `AllowedOrigins` includes the frontend origin when calling the API directly.

### Login succeeds but redirect feels slow

The client preloads authenticated route chunks from Login/Register and skips an immediate duplicate refresh check right after a successful login. If the redirect still feels slow, check:

- Network latency of `POST /authentication/login`.
- Whether the dashboard report endpoint is cold and waiting for DB/cache work.
- Browser DevTools network waterfall for large chunks such as `charts` or `antd`.
- Redis availability when report caching is enabled.

### Dashboard report times out

`/reports/progress` is an expensive endpoint on cache miss.

Recommended checks:

- Ensure Redis is running if `RedisSettings:Enabled=true`.
- Ensure the Redis connection string matches whether Redis uses a password.
- Restart backend after changing user-secrets.
- Check API logs for slow DB queries or Redis connection errors.

### Gmail OTP fails with `535 5.7.8`

Gmail rejected SMTP authentication. Use a Gmail App Password and restart the backend after updating user-secrets.

### Emails silently use placeholder addresses

`appsettings.json` ships with placeholder `EmailSettings` values so the repo has no real secrets in it. If you run the API with `dotnet run` (not Docker) and never set `EmailSettings:*` via `dotnet user-secrets`, the app falls back to those placeholders and SMTP auth fails. Setting values in `.env` has no effect here — see the note in [Configuration](#configuration).

### `Unknown database` on first run

The configured database does not exist yet. Run `dotnet ef database update --project Todo.Infrastructure --startup-project Todo.API` first — EF Core creates the database and applies all migrations.

### Docker Redis password mismatch

If Redis is started with `--requirepass`, the backend connection string must include:

```text
password=your_redis_password
```

If Redis is only bound to `127.0.0.1` for local development, running without a password is acceptable for this project.

## What's Changed

### Unreleased

- Removed hardcoded bootstrap admin email/password from `LoginHandler`; now configured via `Bootstrap:AdminEmail`/`Bootstrap:AdminPassword` (user secrets, `.env`/Docker, or a secret manager in production).

### v2.0.0

- Migrated the backend from Repository Pattern to Clean Architecture: `Todo.Domain`, `Todo.Application`, `Todo.Infrastructure`, `Todo.API`.
- Redesigned repository interfaces to be domain-focused (no `DbSet<T>` or EF Core types in `Todo.Application`); implementations moved to `Todo.Infrastructure/Persistence/Repositories/`.
- Merged `Todo.Models` (DbContext, Identity entities, EF configurations, migrations) into `Todo.Infrastructure/Persistence/`, preserving existing migration history.
- Removed `Todo.Repositories` and `Todo.Services` projects; removed unused `IGenericRepository`/`GenericRepository` dead code from `MayNghien.Infrastructures`.
- Updated `dotnet ef` commands, `Todo.API/Dockerfile`, and project docs to match the new project layout.

### v1.0.0

- Added authentication API wrappers on the React client.
- Added Login, Register, and Change Password pages.
- Added auth illustrations and aligned Login/Register image/form layout.
- Compact Register form into a desktop grid while preserving mobile stacking.
- Added cookie-based private routing and lazy-loaded route boundaries.
- Added authenticated-route preloading from auth pages for faster post-login navigation.
- Avoided an unnecessary immediate refresh-token request right after successful login.
- Added OTP-based registration verification and password-change flow.
- Fixed OTP handling and login redirect behavior on the React client.
- Refined the React UI for mobile, tablet, laptop, and desktop breakpoints.
- Extracted shared SCSS tokens, mixins, page shells, cards, and utility classes.
- Replaced deprecated Sass `@import` usage and split large frontend vendor chunks.
- Added Redis-backed progress report caching with memory fallback.
- Optimized progress report generation to reduce in-memory work.
- Improved local development cookie behavior for direct HTTPS API calls and proxy-based HTTP calls.
- Updated Docker example configuration for Redis password and localhost port exposure.
- Stopped tracking local `docker-compose.yml`; use `docker-compose.yml.example` as the template.
- Cleaned up Ant Design warnings for `Spin`, `Card`, and static `message` usage.

### Earlier

- Todo list and todo item CRUD.
- Dashboard analytics and progress reporting.
- Quartz.NET background jobs for reports, reminders, summaries, and cleanup.
- MySQL persistence with EF Core migrations.
- Docker and Nginx deployment templates.

## Security Notes

- Do not commit `.env`, `docker-compose.yml`, `appsettings.Development.json`, SMTP credentials, JWT secrets, database passwords, or bootstrap admin credentials.
- Rotate any credential that was pasted into chat, terminal logs, or committed history.
- Use Gmail App Passwords for SMTP.
- Keep admin-only endpoints behind role checks and network controls.

## License

This project is licensed under the [MIT License](LICENSE.txt).

## Author

Built by Rainy.
