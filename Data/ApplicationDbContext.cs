using Microsoft.EntityFrameworkCore;
using WorkflowService.Models;

namespace WorkflowService.Data
{
    /// <summary>
    /// Entity Framework Core database context for the workflow microservice.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<WorkflowRecord> Records => Set<WorkflowRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<WorkflowRecord>().HasKey(r => r.Id);
            modelBuilder.Entity<WorkflowRecord>().Property(r => r.State).IsRequired();
            modelBuilder.Entity<WorkflowRecord>().Property(r => r.Name).HasMaxLength(256);
        }
    }
}
