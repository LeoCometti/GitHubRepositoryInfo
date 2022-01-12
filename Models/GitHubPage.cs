using GitHubRepositoryInfo.Extensions;

namespace GitHubRepositoryInfo.Models;

public class GitHubPage
{
    // Fields and constants

    private readonly string pathBranches = "branches";
    private readonly string pathBlob = "blob";
    private readonly string pathTree = "tree";
    private readonly IEnumerable<string> mainBranches = new List<string> { "main", "master" };
    private readonly string searchBranch = "branch=\"";
    private readonly string quotationMark = "\"";

    // DDD factory pattern

    public static GitHubPage Factory(string url)
    {
        var uri = new Uri(url);

        // Validation

        if (uri == null)
            throw new NullReferenceException("Url is not valid.");

        if (uri.Host != "github.com")
            throw new ArgumentException("Url is not from github.");

        if (uri.Segments.Count() < 3)
            throw new ArgumentException("Url does not have all segments required.");

        // Populate variables

        var root = $"{uri.Scheme}://{uri.Host}/";
        var onwer = uri.Segments[1];
        var repo = uri.Segments[2];

        if (repo.Last() != '/')
            repo += "/";

        // Return model

        return new GitHubPage
        {
            Url = url,
            Root = root,
            Onwer = onwer,
            Repo = repo
        };
    }

    // Properties

    public string Url { get; private set; }
    public string Root { get; private set; }
    public string Onwer { get; private set; }
    public string Repo { get; private set; }
    public List<string> Branches { get; private set; }

    // Public Methods

    internal async Task<RepositoryInfo> GetInfo()
    {
        var branchAddress = $"{Root}{Onwer}{Repo}{pathBranches}";

        Branches = GetBranches(branchAddress);

        var targetBranch = mainBranches.Where(x => Branches.Any(y => y == x)).FirstOrDefault();

        if (string.IsNullOrEmpty(targetBranch))
            targetBranch = Branches.FirstOrDefault();

        var repoAddress = $"{Root}{Onwer}{Repo}{pathTree}";
        var filesAddress = $"{Onwer}{Repo}{pathBlob}";
        var foldersAddress = $"{Onwer}{Repo}{pathTree}";

        var fileInfo = GetRepositoryFolderInfo(repoAddress, filesAddress, foldersAddress, targetBranch);

        var repositoryInfo = new RepositoryInfo
        {
            Url = Url,
            FileInfo = fileInfo
        };

        return repositoryInfo;
    }

    // Private Methods

    private List<string> GetBranches(string url)
    {
        var source = GetViewSourceFrom(url);

        var branches = GetListOfStringFrom(source, searchBranch, quotationMark);

        return branches;
    }

    private List<FileInfo> GetRepositoryFolderInfo(string repoAddress, string filesAddress, string foldersAddress, string? currentFolder)
    {
        if (string.IsNullOrEmpty(currentFolder))
            return new List<FileInfo>();

        var url = $"{repoAddress}/{currentFolder}/";

        var source = GetViewSourceFrom(url);

        var searchFiles = $"{filesAddress}/{currentFolder}/";

        var filesName = GetListOfStringFrom(source, searchFiles, quotationMark).Where(x => x.Contains(".") && !x.Contains("/")).Distinct().ToList();

        var searchFolders = $"{foldersAddress}/{currentFolder}/";

        var foldersName = GetListOfStringFrom(source, searchFolders, quotationMark);

        // Get files info

        var fileInfo = GetFileInfo(url, filesName);

        // Update address

        repoAddress += $"/{currentFolder}";
        filesAddress += $"/{currentFolder}";
        foldersAddress += $"/{currentFolder}";

        foreach (var folder in foldersName)
        {
            var filesInsideFolders = GetRepositoryFolderInfo(repoAddress, filesAddress, foldersAddress, folder);

            fileInfo.AddRange(filesInsideFolders);
        }

        return fileInfo;
    }

    private List<FileInfo> GetFileInfo(string url, List<string> filesName)
    {
        var fileInfo = new List<FileInfo>();

        foreach (var fileName in filesName)
        {
            var source = GetViewSourceFrom($"{url}/{fileName}");

            var name = Path.GetFileName(fileName);
            var extension = Path.GetExtension(fileName);
            
            var indexSloc = source.IndexOf("sloc");
            var sloc = source.GetNumberBeforeIndex(indexSloc);

            var indexLines = source.IndexOf("line", indexSloc - 50);
            var lines = source.GetNumberBeforeIndex(indexLines);
            
            var indexBytes = source.IndexOf("</span>", indexLines) + "</span>".Length + 1;
            var bytes = source.GetNumberAndMetricAfterIndex(indexBytes);

            fileInfo.Add(new FileInfo
            {
                Name = name,
                Extension = extension,
                Lines = lines,
                Sloc = sloc,
                Bytes = bytes
            });
        }
        
        return fileInfo;
    }

    private List<string> GetListOfStringFrom(string source, string search, string mark)
    {
        var indexes = source.FindAllOccurrences(search);

        return indexes.Select(x => source.ReturnStringBetween(x + search.Length, mark)).ToList();
    }

    private string GetViewSourceFrom(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            using (HttpResponseMessage response = client.GetAsync(url).Result)
            {
                using (HttpContent content = response.Content)
                {
                    return content.ReadAsStringAsync().Result;
                }
            }
        }
    }
}