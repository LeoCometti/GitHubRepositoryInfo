using System.Diagnostics;
using System.Net;
using GitHubRepositoryInfo.Extensions;
using Flurl.Http;

namespace GitHubRepositoryInfo.Models;

public class GithubPage
{
    // Fields and constants

    private readonly string pathBranches = "branches";
    private readonly string pathBlob = "blob";
    private readonly string pathTree = "tree";
    private readonly IEnumerable<string> mainBranches = new List<string> { "main", "master" };
    private readonly string searchBranch = "branch=\"";
    private readonly string quotationMark = "\"";

    // DDD factory pattern

    public static GithubPage Factory(string url)
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

        return new GithubPage
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
            var swFile = new Stopwatch(); swFile.Start();
            var s1 = new Stopwatch();
            var s2 = new Stopwatch();
            var s3 = new Stopwatch();
            var s4 = new Stopwatch();

            s1.Start();

            var source = GetViewSourceFrom($"{url}/{fileName}");

            var name = Path.GetFileName(fileName);
            var extension = Path.GetExtension(fileName);

            s1.Stop();
            s2.Start();

            var indexSloc = source.IndexOf("sloc");
            var sloc = source.GetNumberBeforeIndex(indexSloc);

            s2.Stop();
            s3.Start();

            var startIndex = indexSloc >= 50 ? indexSloc - 50 : 0;

            var indexLines = source.IndexOf("line", startIndex);
            var lines = source.GetNumberBeforeIndex(indexLines);

            s3.Stop();
            s4.Start();

            var indexBytes = source.IndexOf("</span>", indexLines) + "</span>".Length + 1;
            var bytes = source.GetNumberAndMetricAfterIndex(indexBytes);

            s4.Stop();

            fileInfo.Add(new FileInfo
            {
                Name = name,
                Extension = extension,
                Lines = lines,
                Sloc = sloc,
                Bytes = bytes
            });

            swFile.Stop();

            var timeSpan = swFile.Elapsed.ToString(@"m\:ss\.fff");
            var ts1 = s1.Elapsed.ToString(@"m\:ss\.fff");
            var ts2 = s2.Elapsed.ToString(@"m\:ss\.fff");
            var ts3 = s3.Elapsed.ToString(@"m\:ss\.fff");
            var ts4 = s4.Elapsed.ToString(@"m\:ss\.fff");


            Console.WriteLine($"{fileName} - Time taken: {timeSpan}");
            Console.WriteLine($"S1 - Time taken: {ts1}");
            Console.WriteLine($"S2 - Time taken: {ts2}");
            Console.WriteLine($"S3 - Time taken: {ts3}");
            Console.WriteLine($"S4 - Time taken: {ts4}");
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
        return url.GetStringAsync().Result;

        // using (var webClient = new WebClient())
        // {
        //     webClient.Headers.Add ("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
        //     webClient.Proxy = GlobalProxySelection.GetEmptyWebProxy();

        //     Stream data = webClient.OpenRead(url);
        //     StreamReader reader = new StreamReader (data);
        //     return reader.ReadToEnd();
        // }



        


        // HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(url);
        // //Request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        // Request.Proxy = null;
        // Request.Method = "GET";
        // //Request.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
        // Request.Headers.Add("User-Agent", "C# console program");
        // Request.Headers.Add("Host", "github.com");
        // Request.Headers.Add("Accept", "*/*");
        // Request.Headers.Add("Cookie", "AkamaiEdge=true");
        // using (WebResponse Response = Request.GetResponse())
        // {
        //     using (StreamReader Reader = new StreamReader(Response.GetResponseStream()))
        //     {
        //         return Reader.ReadToEnd();
        //     }
        // }

        // using (var webClient = new WebClient())
        // {
        //     webClient.Proxy = GlobalProxySelection.GetEmptyWebProxy();

        //     return await webClient.DownloadStringTaskAsync(url) ?? string.Empty;
        //     //String url = "http://bg2.cba.pl/realmIP.txt";
        //     //result = webClient.DownloadString(url); // slow as hell
        //     //webClient.OpenRead(url).Read(result, 0, 12); // even slower
        // }

        // var webClient = new WebClient
        // {
        //     Proxy = null,

        // };

        // var result = await webClient.DownloadStringTaskAsync(url);

        // return result ?? string.Empty;

        // HttpClientHandler hch = new HttpClientHandler
        // {
        //     Proxy = null,
        //     UseProxy = false
        // };

        // using (HttpClient client = new HttpClient(hch))
        // {
        //     using (HttpResponseMessage response = client.GetAsync(url).Result)
        //     {
        //         using (HttpContent content = response.Content)
        //         {
        //             return content.ReadAsStringAsync().Result;
        //         }
        //     }
        // }
    }
}