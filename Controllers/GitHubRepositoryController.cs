using System.Diagnostics;
using GitHubRepositoryInfo.Data;
using GitHubRepositoryInfo.Models;
using GitHubRepositoryInfo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GitHubRepositoryInfo.Controllers;

[ApiController]
[Route("[controller]")]
public class GithubRepositoryController : ControllerBase
{
    private readonly ILogger<GithubRepositoryController> _logger;
    private readonly GithubPageInfoService _githubPageInfoService;

    public GithubRepositoryController(ILogger<GithubRepositoryController> logger/*, GithubPageInfoService githubPageInfoService*/)
    {
        _logger = logger;
        //_githubPageInfoService = githubPageInfoService;
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
    public async Task<ActionResult<RepositoryInfo>> Post([FromServices] DataContext context, [FromBody] PostModel post)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var url = post.Url;

            var githubPage = GithubPage.Factory(url);

            var repositoryInfo = await githubPage.GetInfo();

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

    public async Task<IActionResult> Delete([FromServices] DataContext context, [FromBody] DeleteModel delete)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var id = delete.Id;

            var repositoryInfo = await context.RepositoryInfoItems.FirstOrDefaultAsync(x => x.Id == id);

            if (repositoryInfo == null)
            {
                return NotFound();
            }

            context.RepositoryInfoItems.Remove(repositoryInfo);

            await context.SaveChangesAsync();

            return NoContent();
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex.Message);
            return BadRequest(ModelState);
        }
    }
}