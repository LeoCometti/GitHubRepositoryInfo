using System.Diagnostics;
using GitHubRepositoryInfo.Data;
using GitHubRepositoryInfo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GitHubRepositoryInfo.Controllers;

[ApiController]
[Route("[controller]")]
public class GithubRepositoryController : ControllerBase
{
    private readonly ILogger<GithubRepositoryController> _logger;

    public GithubRepositoryController(ILogger<GithubRepositoryController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetGitHubRepository")]
    public async Task<ActionResult<List<RepositoryInfo>>> Get([FromServices] DataContext context)
    {
        var getGitHubRepositoryInfo = await context.RepositoryInfoItems
            .Include(x => x.FileInfo)
            .AsNoTracking()
            .ToListAsync();

        return getGitHubRepositoryInfo;
    }

    [HttpPost(Name = "PostGitHubRepository")]
    public async Task<ActionResult<RepositoryInfo>> Post(
        [FromServices] DataContext context,
        [FromBody] PostModel post)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var url = post.Url;

            var githubPage = GithubPage.Factory(url);

            var swFile = new Stopwatch();
            swFile.Start();

            var repositoryInfo = await githubPage.GetInfo();

            swFile.Stop();
            var timeSpan = swFile.Elapsed.ToString(@"m\:ss\.fff");
            Console.WriteLine($"Url - Time taken: {timeSpan}");

            context.RepositoryInfoItems.Add(repositoryInfo);

            await context.SaveChangesAsync();

            return repositoryInfo;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ModelState);
        }
    }
}