using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitHubRepositoryInfo.Data;
using GitHubRepositoryInfo.Models;

namespace GitHubRepositoryInfo.Services;

public interface IGithubPageInfoService
{
    void AddRequest(string url);
}

public class GithubPageInfoService : BackgroundService, IGithubPageInfoService
{
    private readonly ILogger<GithubPageInfoService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentQueue<string> _pageRequestQueue = new();
    private readonly ConcurrentQueue<int> _deleteItemQueue = new();
    private static readonly AutoResetEvent _semaphore = new(true);
    private static CancellationTokenSource _tokenSource = new();

    public GithubPageInfoService(ILogger<GithubPageInfoService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public void AddRequest(string url)
    {
        _pageRequestQueue.Enqueue(url);

        if (!_semaphore.Set())
            _logger.LogError("Failed to release the semaphore!");
    }

    public void DeleteItem(int id)
    {
        _deleteItemQueue.Enqueue(id);

        if (!_semaphore.Set())
            _logger.LogError("Failed to release the semaphore!");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(ProcessingGithubPageInfoRequests);
    }

    private async void ProcessingGithubPageInfoRequests()
    {
        while (!_tokenSource.IsCancellationRequested)
        {
            try
            {
                if (!_pageRequestQueue.IsEmpty && _pageRequestQueue.TryDequeue(out string url) && !string.IsNullOrEmpty(url))
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var _context = scope.ServiceProvider.GetRequiredService<DataContext>();

                        var githubPage = GithubPage.Factory(url);

                        var repositoryInfo = await githubPage.GetInfo();

                        _context.RepositoryInfoItems.Add(repositoryInfo);

                        await _context.SaveChangesAsync();
                    }
                }

                if (!_deleteItemQueue.IsEmpty && _deleteItemQueue.TryDequeue(out int id))
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var _context = scope.ServiceProvider.GetRequiredService<DataContext>();

                        var repositoryInfo = _context.RepositoryInfoItems.FirstOrDefault(x => x.Id == id);

                        if (repositoryInfo != null)
                        {
                            _context.RepositoryInfoItems.Remove(repositoryInfo);

                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            _logger.LogWarning($"Item does not exist. Id = {id}");
                        }
                    }
                }

                if (_pageRequestQueue.IsEmpty && _deleteItemQueue.IsEmpty)
                    _semaphore.WaitOne();

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error {ex.Message}");
            }
        }
    }
}