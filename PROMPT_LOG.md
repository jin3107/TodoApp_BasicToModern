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

## Manual Changes

### Change 1: Add DisallowConcurrentExecution Attribute
**Location:** Todo.Services/Jobs/ClearExpiredDataJob.cs

**What Changed:**
Added [DisallowConcurrentExecution] attribute to ClearExpiredDataJob class.

**Reasoning:**
Three other jobs had this attribute but ClearExpiredDataJob was missing it. Added for consistency and to prevent the job from running multiple times on the same instance.
