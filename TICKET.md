# [TASK] Scale Quartz Job Scheduler for Horizontal Deployment

| Field        | Value                                              |
|--------------|----------------------------------------------------|
| **Type**     | Task                                               |
| **Points**   | 3                                                  |
| **Assignee** | Carter Frank                                       |
| **Priority** | High                                               |
| **Labels**   | backend, scaling, quartz, infrastructure           |
| **Repo**     | https://github.com/jin3107/TodoApp_BasicToModern   |

---

## Summary

The app uses `RAMJobStore` for Quartz scheduling. Under multiple replicas, every node fires every job independently — causing duplicate emails. This task migrates to `AdoJobStore` (MySQL) with clustering enabled, so only one node fires per trigger. Several pre-existing bugs in the email and controller layers are also in scope since they become critical failures at scale.

---

## Files

| Path | Action |
|------|--------|
| `TodoApp.Server/src/Todo.API/Todo.API.csproj` | Modify — add `Quartz.Jobs` and `MySql.Data` package references |
| `TodoApp.Server/src/Todo.API/Extensions/QuartzServiceExtensions.cs` | Modify — replace `UseInMemoryStore` with `UsePersistentStore` + cluster config |
| `TodoApp.Server/src/Todo.API/appsettings.json` | Modify — remove dead config key; add cluster flag |
| `TodoApp.Server/src/Todo.API/Controllers/JobsController.cs` | Modify — inject `IScheduler` directly; remove `.Result` deadlock |
| `TodoApp.Server/src/Todo.Services/Implementations/EmailService.cs` | Modify — fix silent weekly send bug; rethrow on failure; `DateTime.UtcNow` |
| `TodoApp.Server/src/Todo.Models/Migrations/QuartzSchema_MySQL.sql` | **Create** — idempotent Quartz 3.x MySQL DDL (`QRTZ_*` tables) |

---

## Reproduction

Local environment: single API replica, MySQL 8.0, Redis disabled (memory fallback).

**Check 1 — Store is non-clustered**

Startup logs on `dotnet run`:
```
RAMJobStore initialized.
Using job store 'Quartz.Simpl.RAMJobStore', supports persistence: False, clustered: False
Quartz Scheduler initialized with instanceId 'NON_CLUSTERED'
```

**Check 2 — Weekly job silently no-ops**

```bash
curl -s -X POST http://localhost:5133/jobs/trigger/weekly-summary
# {"message":"Weekly summary job triggered successfully!"}
```

Observed log sequence:
```
info: Seding weekly todo item summary at 04/15/2026 19:16:09
info: [EF Core] Executed DbCommand — SELECT from TodoItems
info: Cached get progress result for key report:progress:...
info: Weekly Todo Item Summary Job completed successfully!
```

Report data was fetched and the email body was built. No SMTP attempt appears anywhere in the log. The job returned `200 OK` and logged success with no indication of failure. The email was never sent.

---

## Acceptance Criteria

- [ ] Two API replicas produce exactly one email per scheduled trigger, not two
- [ ] Node restarts don't silently drop misfires — shared store handles misfire tracking
- [ ] Weekly summary job actually sends an email (currently a silent no-op)
- [ ] Weekly summary job propagates exceptions so Quartz retry applies
- [ ] `JobsController` resolves `IScheduler` without a synchronous `.Result` block
- [ ] All log timestamps use UTC
- [ ] Quartz schema migration script is committed and idempotent (`CREATE TABLE IF NOT EXISTS`)
- [ ] `appsettings.json` is the single source of truth for Quartz config
