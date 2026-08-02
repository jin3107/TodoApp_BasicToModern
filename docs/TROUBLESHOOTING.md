# Troubleshooting

See the main [README](../README.md) for setup and configuration.

## Login succeeds but the app returns to `/login`

Check whether `RefreshToken` is stored and sent by the browser. In development, cookie settings depend on whether requests are made through HTTP proxy or direct HTTPS backend calls.

Also verify:

- `withCredentials: true` is enabled in Axios.
- Backend was restarted after cookie configuration changes.
- Old localhost cookies were cleared.
- `AllowedOrigins` includes the frontend origin when calling the API directly.

## Login succeeds but redirect feels slow

The client preloads authenticated route chunks from Login/Register and skips an immediate duplicate refresh check right after a successful login. If the redirect still feels slow, check:

- Network latency of `POST /authentication/login`.
- Whether the dashboard report endpoint is cold and waiting for DB/cache work.
- Browser DevTools network waterfall for large chunks such as `charts` or `antd`.
- Redis availability when report caching is enabled.

## Dashboard report times out

`/reports/progress` is an expensive endpoint on cache miss.

Recommended checks:

- Ensure Redis is running if `RedisSettings:Enabled=true`.
- Ensure the Redis connection string matches whether Redis uses a password.
- Restart backend after changing user-secrets.
- Check API logs for slow DB queries or Redis connection errors.

## Gmail OTP fails with `535 5.7.8`

Gmail rejected SMTP authentication. Use a Gmail App Password and restart the backend after updating user-secrets.

## Emails silently use placeholder addresses

`appsettings.json` ships with placeholder `EmailSettings` values so the repo has no real secrets in it. If you run the API with `dotnet run` (not Docker) and never set `EmailSettings:*` via `dotnet user-secrets`, the app falls back to those placeholders and SMTP auth fails. Setting values in `.env` has no effect here — see the Configuration section in the [README](../README.md#configuration).

## `Unknown database` on first run

The configured database does not exist yet. Run `dotnet ef database update --project Todo.Infrastructure --startup-project Todo.API` first — EF Core creates the database and applies all migrations.

## Docker Redis password mismatch

If Redis is started with `--requirepass`, the backend connection string must include:

```text
password=your_redis_password
```

If Redis is only bound to `127.0.0.1` for local development, running without a password is acceptable for this project.
