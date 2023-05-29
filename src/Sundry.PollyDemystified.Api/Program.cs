using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Sundry.PollyDemystified.Api.Interface;
using Sundry.PollyDemystified.Core.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Services.AddSingleton<ITodoRepository, TodoRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TODO API",
        Description = "Web APIs for managing a TODO list",
        Version = "v1"
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
     .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
     {
         c.Authority = builder.Configuration["Auth0:Domain"];
         c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
         {
             ValidAudience = builder.Configuration["Auth0:Audience"],
             ValidIssuer = builder.Configuration["Auth0:Domain"]
         };
     });

builder.Services.AddAuthorization(o =>
    {
        o.AddPolicy("todo:read-write", p => p.
            RequireAuthenticatedUser().
            RequireClaim("scope", "todo:read-write"));
    });

var app = builder.Build();
app.MapSwagger();
app.UseSwaggerUI();

app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (BadHttpRequestException ex)
    {
        ctx.Response.StatusCode = ex.StatusCode;
        await ctx.Response.WriteAsync(ex.Message);
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("");

app.MapPost("/todo", async (
    HttpRequest req,
    ITodoRepository repo) =>
{
    var todo = await req.ReadFromJsonAsync<TodoItem>();

    if (string.IsNullOrWhiteSpace(todo?.Description))
    {
        return Results.BadRequest("Description is required");
    }

    var id = await repo.CreateAsync(todo.Description);
    return Results.Accepted($"/todo/{id}");
}).RequireAuthorization("todo:read-write");

app.MapGet("/todo", async (ITodoRepository repo) =>
{
    var todos = await repo.GetAllAsync();
    return Results.Ok(todos);
}).RequireAuthorization("todo:read-write");

app.MapGet("/todo/{id}", async (string id, ITodoRepository repo) =>
{
    if (string.IsNullOrWhiteSpace(id))
    {
        return Results.BadRequest("id is required");
    }
    return Results.Ok(await repo.Get(id)) ?? Results.BadRequest("item not found");
}).RequireAuthorization("todo:read-write");

app.MapPut("/todo/{id}", async (string id, HttpRequest req, ITodoRepository repo) =>
{
    if (!req.HasJsonContentType())
    {
        return Results.BadRequest("Only application/json supported");
    }

    var todo = await req.ReadFromJsonAsync<TodoItem>();

    if (todo?.Completed != true)
    {
        return Results.BadRequest("Completed is required");
    }
    await repo.Update(id, todo.Completed);
    return Results.Ok();
}).RequireAuthorization("todo:read-write");

app.MapDelete("/todo/{id}", async (string id, ITodoRepository repo) =>
{
    if (string.IsNullOrWhiteSpace(id))
    {
        return Results.BadRequest("id is required");
    }
    await repo.Delete(id);
    return Results.Ok();
}).RequireAuthorization("todo:read-write");

app.Run();
