# Changelog

All notable changes to this project are documented here. See the main [README](README.md) for setup and usage.

## Unreleased

_Nothing yet._

## v1.2.0

- Redesigned the entire frontend with a new "Classical" design system: serif headings, Lora body text, a single warm-gold accent with a tonal ramp, small consistent border radii (2/4/7px), and hairline dividers — replacing the previous default Ant Design look.
- Added an app-wide light/dark theme toggle (`ThemeContext` + a dynamic Ant Design `ConfigProvider` theme), with a toggle in the main header.
- Reworked Todo Lists and Todo Items: inline create/edit instead of modals, internal scroll instead of pagination, immediate checkbox/priority/due-date edits, and undo toasts for item deletion (list deletion keeps its confirm dialog plus an undo toast, since it cascades to all items in the list).
- Wired the Reports page to a `/reports` route (previously built but not routed) and rebuilt it and the Dashboard with lightweight SVG/CSS charts — a line chart, a conic-gradient donut, and div-based bars — removing the `@ant-design/charts` dependency from these views and shrinking the bundle accordingly.
- Restructured Login and Register into a centered single-card layout with a shared `AuthShell` component, keeping the auth illustration alongside the form.
- Rebuilt the main navigation (underline-active nav items, global theme toggle button, new "Báo cáo" link) and removed the previous fixed-dark navbar background so it follows the active theme.
- Removed hardcoded bootstrap admin email/password from `LoginHandler`; now configured via `Bootstrap:AdminEmail`/`Bootstrap:AdminPassword` (user secrets, `.env`/Docker, or a secret manager in production).

## v1.1.0

- Migrated the backend from Repository Pattern to Clean Architecture: `Todo.Domain`, `Todo.Application`, `Todo.Infrastructure`, `Todo.API`.
- Redesigned repository interfaces to be domain-focused (no `DbSet<T>` or EF Core types in `Todo.Application`); implementations moved to `Todo.Infrastructure/Persistence/Repositories/`.
- Merged `Todo.Models` (DbContext, Identity entities, EF configurations, migrations) into `Todo.Infrastructure/Persistence/`, preserving existing migration history.
- Removed `Todo.Repositories` and `Todo.Services` projects; removed unused `IGenericRepository`/`GenericRepository` dead code from `MayNghien.Infrastructures`.
- Updated `dotnet ef` commands, `Todo.API/Dockerfile`, and project docs to match the new project layout.

## v1.0.0

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

## Earlier

- Todo list and todo item CRUD.
- Dashboard analytics and progress reporting.
- Quartz.NET background jobs for reports, reminders, summaries, and cleanup.
- MySQL persistence with EF Core migrations.
- Docker and Nginx deployment templates.
