using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ToDoApp.Database;
using ToDoApp.Models;

namespace ToDoApp;

public class Startup(IConfiguration configuration)
{
    /// <summary>
    /// Add services to the container. 
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services) 
    {
        // Define the SQLite database path
        var dbPath = Path.Combine(Environment.CurrentDirectory, "ToDoDatabase.db");
        services.AddDbContext<ToDoContext>(opt =>
            opt.UseSqlite($"Data Source={dbPath}"));

        // Add services for Swagger.
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo {  Title = "ToDo API", Version = "v1" });
        });
    }

    /// <summary>
    /// Configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="environment"></param>
    public void Configure(WebApplication app, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        
        // Define endpoints
        app.MapGet("/api/todo", async (ToDoContext db) =>
            await db.ToDoItems.ToListAsync());

        app.MapGet("/api/todo/{id}", async (int id, ToDoContext db) =>
            await db.ToDoItems.FindAsync(id)
                is ToDoItem todo
                    ? Results.Ok(todo)
                    : Results.NotFound());

        app.MapPost("/api/todo", async (ToDoItem todo, ToDoContext db) =>
        {
            db.ToDoItems.Add(todo);
            await db.SaveChangesAsync();

            return Results.Created($"/api/todo/{todo.Id}", todo);
        });

        app.MapPut("/api/todo/{id}", async (int id, ToDoItem inputTodo, ToDoContext db) =>
        {
            var todo = await db.ToDoItems.FindAsync(id);

            if (todo is null) return Results.NotFound();

            todo.Title = inputTodo.Title;
            todo.IsCompleted = inputTodo.IsCompleted;

            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        app.MapDelete("/api/todo/{id}", async (int id, ToDoContext db) =>
        {
            if (await db.ToDoItems.FindAsync(id) is ToDoItem todo)
            {
                db.ToDoItems.Remove(todo);
                await db.SaveChangesAsync();
                return Results.NoContent();
            }

            return Results.NotFound();
        });
    }
}
