# AI Prompt Log - Quartz Horizontal Scaling Assessment

## Prompt 1

**Prompt:**
```
I am working to horizontally scale Quartz.NET jobs. I explored the Quartz 
setup and found: QuartzServiceExtensions.cs using in-memory store, 
4 scheduled jobs defined with cron schedules, and JobsController which 
allows manual triggering. 

Explore the repo and tell me what happens when this application scales 
to multiple instances. What breaks with this architecture and why? Are there 
any other areas I should examine related to this issue?
```

**Reasoning:**
Initial problem introduction and repo exploration to identify what breaks when scaling horizontally.

**Outcome:**
Discovered in-memory store causes duplicate execution and no coordination between instances. ClearExpiredDataJob missing DisallowConcurrentExecution attribute. MySQL database can support persistent job store with clustering.

---

## Prompt 2

**Prompt:**
```
Walk me through the trade offs of using ADO.NET persistent job store 
with clustering. What complexity are we adding versus the problem we are 
solving? Is there a simpler or better alternative?ß
```

**Reasoning:**
Solution evaluation to understand trade offs and complexity of clustering approach.

**Outcome:**
AI suggested separate scheduler service with feature flags as a scale-adaptive approach. This allows starting simple with one instance running scheduler, then separating into dedicated service as scale grows. May be over engineering.

---

## Prompt 3

**Prompt:**
```
The feature flag approach seems like over engineering for this ticket scope. 
At what scale does ADO.NET clustering become insufficient?
```

**Reasoning:**
Scope check on solution complexity and scale limits of ADO.NET clustering.

**Outcome:**
ADO.NET clustering is sufficient for deployments up to 100+ instances with 1000+ jobs. For the current 4 jobs, it handles the problem easily. Implementation requires adding NuGet package, updating QuartzServiceExtensions to use persistent store with clustering, and running SQL schema script to create 11 Quartz tables.

---

## Prompt 4

**Prompt:**
```
Before implementation, how will I test that ADO.NET clustering is working 
correctly? What should I check to verify jobs run only once across multiple 
instances?
```

**Reasoning:**
Testing strategy to verify clustering prevents duplicate job execution.

**Outcome:**
Testing approach includes running multiple local instances, checking database state in Quartz tables, and manual trigger tests. Key checks are unique instance IDs in scheduler info, multiple rows in QRTZ_SCHEDULER_STATE table, and one execution per trigger across all instances.

---

## Prompt 5

**Prompt:**
```
Implement the ADO.NET clustering changes for Quartz.NET. Implementation TODO:
1. Add Quartz.Serialization.SystemTextJson NuGet package to Todo.API project
2. Update QuartzServiceExtensions.cs to replace UseInMemoryStore() with 
   UsePersistentStore() using MySQL clustering config
3. Update appsettings.json if needed

Plan your approach, make changes to the files, then review implementation 
thoroughly. Focus on configuration only, not SQL schema execution.

If you have questions or need clarification, ask before making assumptions.
```

**Reasoning:**
Implementation of ADO.NET clustering configuration with review.

**Outcome:**
AI asked three clarifying questions about connection string format (MySQL vs SQL Server syntax), confirming no schema initialization code needed, and whether to add testing-specific configuration.

---

## Prompt 6

**Prompt:**
```
1. Leave the conn str as placeholder with SQL server syntax.
2. Correct, no schema initialization code
3. No testing specific config needed, use the same config

Go ahead with implementation
```

**Reasoning:**
Answered questions about conn string format and scope.

**Outcome:**
AI implemented clustering config changes. Added two NuGet packages, updated QuartzServiceExtensions.cs to use persistent store with MySQL and clustering enabled, updated Program.cs to pass config, and cleaned up appsettings.json. Implementation reviewed and validated (besides dev environment testing).

---

## Manual Changes

### Change 1: Add DisallowConcurrentExecution Attribute
**Location:** Todo.Services/Jobs/ClearExpiredDataJob.cs

**What Changed:**
Added [DisallowConcurrentExecution] attribute to ClearExpiredDataJob class.

**Reasoning:**
Three other jobs had this attribute but ClearExpiredDataJob was missing it. Added for consistency and to prevent the job from running multiple times on the same instance.
