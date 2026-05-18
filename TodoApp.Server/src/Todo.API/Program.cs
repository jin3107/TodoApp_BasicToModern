using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Todo.API;
using Todo.Models.Data;
using Todo.Models.Entities;
using Quartz;
using Todo.Services.Implementations;
using Todo.API.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Infrastructure
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddRedisCache(builder.Configuration);

// Business layers
builder.Services.AddRepositories();
builder.Services.AddApplicationServices(); 

// Jobs
builder.Services.AddQuartzConfiguration();

// API concerns
builder.Services.AddSwaggerConfiguration();
builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddIdentityConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);


builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
