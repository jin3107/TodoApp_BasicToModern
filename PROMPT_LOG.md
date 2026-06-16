# AI Prompt Log - Quartz Horizontal Scaling Assessment

**Models Used:**
- Claude Sonnet 4.5 (Prompts 1-6): Analysis and implementation
- Claude Opus 4.7 (Prompt 7): Final review

---

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

## Prompt 7

**Prompt:**
```
Review my changes in this branch to implement Quartz horizontal scaling using ADO.NET:
1. Read TICKET.md for context
2. Read PROMPT_LOG.md and make sure it accurately represents the work done
3. Look at git diff for code changes

Look for any issues or improvements needed. If you find something to change or fix, justify that change/fix.
```

**Reasoning:**
Final review using better model to catch any issues before ticket submission.

**Outcome:**
Found two critical bugs: 1) instanceId config in appsettings.json is not being read (needs to be set in code or bound via QuartzOptions), causing all instances to use default "NON_CLUSTERED" ID which breaks clustering. 2) ClearExpiredDataJob trigger has no stable identity, creating duplicate triggers on each restart with persistent store. Also proposed optional improvement to use MySqlConnector instead of MySql.Data to avoid using two seperate MySQL drivers.

---

## Prompt 8

**Prompt:**
```
Yes, apply fixes 1 and 2. Set SchedulerName and SchedulerId in code, add stable 
identity to cleanup trigger, and remove the dead Quartz json section. Don't fix 3 
(the MySqlConnector)
```

**Reasoning:**
Fix critical bugs found in review that would break clustering in prod.

**Outcome:**
Applied both fixes. Set SchedulerName and SchedulerId in code to enable unique instance IDs for clustering. Added stable TriggerKey to ClearExpiredDataJob trigger to prevent duplicates on restart. Removed dead Quartz JSON section from appsettings.json.

---

## Manual Changes

### Change 1: Add DisallowConcurrentExecution Attribute
**Location:** Todo.Services/Jobs/ClearExpiredDataJob.cs

**What Changed:**
Added DisallowConcurrentExecution attribute to ClearExpiredDataJob class.

**Reasoning:**
Three other jobs had this attribute but ClearExpiredDataJob was missing it. Added to prevent the job from running multiple times on the same instance.

---

### Change 2: Add Job Description to ClearExpiredDataJob
**Location:** Todo.API/Extensions/QuartzServiceExtensions.cs

**What Changed:**
Added WithDescription() to ClearExpiredDataJob configuration.

**Reasoning:**
All other jobs had descriptions but ClearExpiredDataJob was missing one. Added for consistency when looking at job details.
