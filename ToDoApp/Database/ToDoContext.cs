using Microsoft.EntityFrameworkCore;
using ToDoApp.Models;

namespace ToDoApp.Database;

public class ToDoContext : DbContext
{
    public ToDoContext(DbContextOptions<ToDoContext> options)
        : base(options)
    {
    }

    public DbSet<ToDoItem> ToDoItems { get; set; }
}
