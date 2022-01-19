using GitHubRepositoryInfo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GitHubRepositoryInfo.ModelMap
{
    public class FileInfoMap : IEntityTypeConfiguration<Models.FileInfo>
    {
        public void Configure(EntityTypeBuilder<Models.FileInfo> builder)
        {
            builder
                .HasOne<RepositoryInfo>()
                .WithMany(x => x.FileInfo);
        }
    }
}