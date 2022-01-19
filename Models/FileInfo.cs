using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GitHubRepositoryInfo.Models;

public class FileInfo
{
    [Key]
    public long Id { get; protected set; }
    public string Name { get; set; }
    public string Extension { get; set; }
    public string Lines { get; set; }
    public string Sloc { get; set; }
    public string Bytes { get; set; }

    [ForeignKey("RepositoryInfo_Id")]
    public long RepositoryInfoId { get; private set; }
}