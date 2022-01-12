using GitHubRepositoryInfo.Data;
using GitHubRepositoryInfo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GitHubRepositoryInfo.Controllers;

[ApiController]
[Route("[controller]")]
public class GitHubRepositoryController : ControllerBase
{
    private readonly ILogger<GitHubRepositoryController> _logger;

    public GitHubRepositoryController(ILogger<GitHubRepositoryController> logger)
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

            var gitHubPage = GitHubPage.Factory(url);

            var repositoryInfo = await gitHubPage.GetInfo();

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