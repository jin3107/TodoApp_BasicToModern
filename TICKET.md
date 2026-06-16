# [TICKET-001] Enable Horizontal Scaling for Quartz.NET Background Jobs

**Priority:** High  
**Status:** In Review

## Summary
Enable Quartz.NET job clustering to support horizontal scaling across multiple instances without job duplication.

---

## Problem Statement

**Current State:**
- Quartz.NET uses in-memory RAMJobStore
- Each instance has its own separate job store
- No coordination between instances

**Impact:**
- All 4 jobs run on every instance (duplicate execution)
- Users receive multiple identical emails
- Race conditions in cleanup operations
- Manual triggers only affect one random instance

**Root Cause:**
- `UseInMemoryStore()` creates isolated job stores per instance with no shared state

---

## Solution

**Approach:** Replace in-memory store with ADO.NET persistent store using MySQL with clustering.

**Why:**
- Coordinates job execution across instances through shared database
- Scales to 100+ instances with 1000+ jobs
- Minimal overhead: 11 database tables, 10-20 queries/min/instance

**Limitations:**
- If scale exceeds limits (ie 150+ instances), consider separate scheduler service with feature flags

---

## Implementation

### Code Changes

**1. NuGet Packages**
- Added `MySql.Data` and `Quartz.Serialization.SystemTextJson` to Todo.API.csproj

**2. QuartzServiceExtensions.cs**
- Replaced `UseInMemoryStore()` with `UsePersistentStore()`
- Configured MySQL with clustering (20s checkin interval)
- Set `SchedulerName` and `SchedulerId: AUTO` in code for unique instance IDs
- Added stable TriggerKey to ClearExpiredDataJob trigger

**3. Program.cs**
- Pass configuration to `AddQuartzConfiguration()`

**4. appsettings.json**
- Removed Quartz configuration section (now set in code)

**5. ClearExpiredDataJob.cs**
- Added missing `[DisallowConcurrentExecution]` attribute

### Database Setup

**Required:** Run MySQL schema script to create 11 Quartz tables before deployment.

**Script:** https://github.com/quartznet/quartznet/blob/main/database/tables/tables_mysql_innodb.sql

### How It Works

- Each instance registers with unique ID in `QRTZ_SCHEDULER_STATE`
- Instances send heartbeat every 20 seconds
- When job fires, instances compete for lock in `QRTZ_LOCKS` table
- Only one instance acquires lock and runs the job
- Dead instances detected after 40 seconds, jobs recovered

---

## Testing

**Quick Verification:**
1. Check `/jobs/scheduler/info` on each instance shows different `instanceId`
2. Query `QRTZ_SCHEDULER_STATE` table shows all running instances
3. Trigger job manually, verify only one instance executes
4. Check single email received instead of multiple

**Database Checks:**
```sql
-- All instances registered
SELECT INSTANCE_NAME, LAST_CHECKIN_TIME FROM QRTZ_SCHEDULER_STATE;

-- No dupe jobs
SELECT JOB_NAME, COUNT(*) FROM QRTZ_JOB_DETAILS GROUP BY JOB_NAME HAVING COUNT(*) > 1;
```

---

## Deployment

**Prereqs:**
1. Run Quartz schema script on MySQL database
2. Verify MySQL user has SELECT, INSERT, UPDATE, DELETE on QRTZ_* tables

**Steps:**
1. Deploy updated code to all instances
2. Instances auto-register and coordinate via database
3. Monitor `QRTZ_SCHEDULER_STATE` table

**Rollback:**
- Revert to previous deployment
- In-memory store resumes (dupes return but app functional)

---

## References
- Quartz.NET Clustering: https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/job-stores.html
- MySQL Schema: https://github.com/quartznet/quartznet/blob/main/database/tables/tables_mysql_innodb.sql
