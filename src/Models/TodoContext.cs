using Microsoft.EntityFrameworkCore;

namespace TodoApi.Models
{
    public class TodoContext(DbContextOptions<TodoContext> options) : DbContext(options)
    {
        public DbSet<TodoItem> TodoItems { get; set; }

        public DbSet<TodoItemValue> TodoItemValues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TodoItem>().HasMany(e => e.TodoItemValues)
                .WithOne().HasForeignKey(e => e.TodoItemId).IsRequired(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}