using AsyncHealthChecker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AsyncHealthChecker.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<CheckResult> CheckResults => Set<CheckResult>();
    public DbSet<CheckTask> CheckTasks => Set<CheckTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CheckTask>()
            .HasKey(t => t.TaskId);
    }
}