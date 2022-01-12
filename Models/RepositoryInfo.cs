using System.ComponentModel.DataAnnotations;

namespace GitHubRepositoryInfo.Models;

public class RepositoryInfo
{
    [Key]
    public long Id { get; set; }
    public string Url { get; set; }
    public List<FileInfo> FileInfo { get; set; } = new();
}