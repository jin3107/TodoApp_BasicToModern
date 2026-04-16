# Prompt Log — Quartz Horizontal Scaling

A running log of AI prompts used during this technical assessment, with intent notes.

---

## Prompt 1

I need  to contextualize myself with the direction of this specification for this task I have been assigned.                                                                                                                  
                                                                                                                                                                                                      
  Heres my understanding thus far:                                                                                                                                                                    
                                                                                                                                                                                                      
  I am doing a network architecture scaling (perhaps dynamically) inside a todo application with a database layer, caching layer, as well as basic client-server architecture                         
  I need to checkout a new branch feature/compute-scaling or something similar to safely develop                                                                                                      
  I think I can omit docker deployments for now as I am not working in production env (correct if im wrong)                                                                                           
  I need to log my prompts (need to do as we work) and need to develop a jira-ticket (in the end)                                                                                                     
                                                                                                                                                                                                      
  Heres what I need help understanding so far:                                                                                                                                                        
                                                                                                                                                                                                      
  Not entirely familiar with quartz jobs                                                                                                                                                              
  I need to look for focal points in the codebase. I have scanned it and it is apparent I am not working in client other than maybe refactoring a fetch. I do not need to mess with reverse proxying  
  and most likely caching.                                                                                                                                                                            
                                                                                                                                                                                                      
  My goal for now is just contextualization and logging my prompts in a PROMPT_LOG.md once I checkout a new branch with the designated name.

---

## Prompt 2 

We have scoped the problem and identified the focal points and zoomed in our scope for this feature. Lets draft a ticket:                                                                           
                                                                                                                                                                                                      
  The issue type is going to most likely be a story or task as we're in between feature and optimization upgrade.                                                                                     
  We have a list of files we're touching:                                                                                                                                                             
                                                                                                                                                                                                      
  JobController.cs, QuartsServiceExtensions.cs, EmailService.cs, appsettings.json, Toso.API.csproj (go ahead and list full relative paths from root in the jira ticket draft)                         
  The description is mainly as follows from the assessment spec:                                                                                                                                      
                                                                                                                                                                                                      
  - You've just joined the team that owns this application: https://github.com/jin3107/TodoApp_BasicToModern                                                                                          
  - It's a full-stack Todo application currently running in a small internal deployment                                                                                                               
  - Leadership has flagged it as a scaling concern usage is growing rapidly and the current architecture won't hold up.                                                                               
                                                                                                                                                                                                      
                                                                                                                                                                                                      
  Logically, we're dealing with the backend components of the architecture, and optimizing Quartz jobs.                                                                                               
                                                                                                                                                                                                      
  Give me feedback if I'm missing critical components of the ticket. The size is most likely going to be 3 following fibbonacci rule                                                                  
  I am the assignee (carter frank) 

## Prompt 3

  Lets move into generating a testable environment. Go through the readme and itemize a list of actions for me to do to setup a testing environment. Keep these constraints in mind:                  
                                                                                                                                                                                                      
  I do not have database credentials, nor internal artifactory docker authorization. I am purely making a performance change in my git branch and I need to be able to verify the output.   


## Prompt 4

give me curl commands for the api endpoints that we are running from the dotnet api right now to help me recreate the issue in the current platform

## Prompt 5

We have our problem scope, and a number of things we should change the behavior for. Itemize this into a list and order them with dependency on another in mind. Do not make changes yet. Be verbose  
  logically but not too specific on the code itself.   

## Prompt 6

Explain the key differences in going from the baseline state of RAMJobStore to AdoJobStore

## Prompt 7

With the goal state and baseline state in mind, what test cases can we implement that fail now and pass after a solution is implemented.

## Prompt 8

In this new directory Todo.Tests there are two files that need to be populated with the discussed cases, implement the test cases and I will verify they currently fail.

## Prompt 9

Help me get the ball rolling implemented code. Implement these in sequence and I will verify by running dotnet restore on the api:

    Remove the dead quartz.jobStore.type key pointing at RAMJobStore
    Add quartz.jobStore.clustered: true

## Prompt 10

I now need to enable the clustering mode by replacing the RAMJobStore with AdoJobStore. Move all scheduler states into the database and out of the process. We should treat these process nodes as workers only and not schedulers too. Let the database handle the scheduling. Suggest changes to our current version of QuartzServiceExtensions.cs. I will lastly verify that our changes still allow the API to be built with no errors.

## Prompt 11

Add date and time stamps to the logging, and adjust the class declarations in JobsController.cs 

## Prompt 12

We are not catching the error in the weekly task summary job. Apply these fixes:

    add the missing SendEmailAsync call with subject and recipient, and the "sent successfully" log line after it
    add throw to the catch block so WeeklyTaskSummaryJob receives the exception

## Prompt 13

The standalone SQL script we used to provision the Quartz tables during local setup needs a permanent home. Walk me through embedding it into an EF Core migration so that dotnet ef database update handles both the application schema and the Quartz schema in one step, and the manual docker exec prerequisite is eliminated.

**Intent:** The `QuartzSchema_MySQL.sql` file was a dev shortcut, not a production solution. Moving it into a migration ties it to the project's versioned schema history, removes a manual deployment step, and directly supports the scaling goal — new nodes can come up without any out-of-band setup.
