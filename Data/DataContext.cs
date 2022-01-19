using GitHubRepositoryInfo.ModelMap;
using GitHubRepositoryInfo.Models;
using Microsoft.EntityFrameworkCore;

namespace GitHubRepositoryInfo.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {

    }

    public DbSet<RepositoryInfo> RepositoryInfoItems { get; set; } = null!;
    public DbSet<Models.FileInfo> FileInfoItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new FileInfoMap());

        modelBuilder.Entity<RepositoryInfo>()
                .HasMany(x => x.FileInfo)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
    }
}