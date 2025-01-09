using Microsoft.EntityFrameworkCore;
using WebTV.Models;

namespace WebTV.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<User> Users { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)    
        {
            modelBuilder.Entity<TodoItem>()
                .ToContainer("webtvContainer")
                .HasPartitionKey(x => x.Id)
                .HasDiscriminator<string>("EntityType") 
                .HasValue<TodoItem>("TodoItem");

            modelBuilder.Entity<User>()
                .ToContainer("webtvContainer")
                .HasPartitionKey(x => x.Id)
                .HasDiscriminator<string>("EntityType")
                .HasValue<User>("User"); 

            modelBuilder.Entity<TodoItem>().HasKey(x => x.Id);
            modelBuilder.Entity<User>().HasKey(x => x.Id);
        }
    }
}
