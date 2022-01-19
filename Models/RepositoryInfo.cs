using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace GitHubRepositoryInfo.Models;

public class RepositoryInfo
{
    [Key]
    public long Id { get; protected set; }
    public string Url { get; set; }
    public virtual ICollection<FileInfo> FileInfo { get; set; } = new Collection<FileInfo>();
}