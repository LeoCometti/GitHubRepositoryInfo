using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitHubRepositoryInfo.Services;

public interface IGithubPageInfoService
{
    void AddRequest(string url);
}

public class GithubPageInfoService : BackgroundService, IGithubPageInfoService
{
    private readonly ILogger<GithubPageInfoService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentQueue<string> pageRequestQueue = new();
    private static CancellationTokenSource _tokenSource = new();

    public GithubPageInfoService(ILogger<GithubPageInfoService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public void AddRequest(string url)
    {
        pageRequestQueue.Enqueue(url);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(ProcessingGithubPageInfoRequests);
    }

    private async void ProcessingGithubPageInfoRequests()
    {
        while (!_tokenSource.IsCancellationRequested)
        {
            int x = 0;

            Thread.Sleep(1000);
        }
    }
}