using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Todo.API;
using Todo.Models.Data;
using Todo.Models.Entities;
using Todo.API.Extensions;
using Todo.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDatabaseConfiguration(builder.Configuration);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

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
