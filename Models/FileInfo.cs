using System.ComponentModel.DataAnnotations;

namespace GitHubRepositoryInfo.Models;

public class FileInfo
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; }
    public string Extension { get; set; }
    public string Lines { get; set; }
    public string Sloc { get; set; }
    public string Bytes { get; set; }
}