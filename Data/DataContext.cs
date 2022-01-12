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
}