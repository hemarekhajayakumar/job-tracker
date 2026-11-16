using JobTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Api.Data;

// The DbContext is our doorway to the database.
// Each DbSet below is one table, and we query it with C# instead of SQL.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Two users cannot register with the same email.
        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        // Save the status as text ("Applied") instead of a number (0), so the table is readable.
        modelBuilder.Entity<JobApplication>()
            .Property(job => job.Status)
            .HasConversion<string>();
    }
}
