using GitsCommander.Infrastructure;

namespace GitsCommander.RepoListAggregate;

record RepoListCommandHandler(WorkItemEngine engine, RepositoriesLogic fetcher, IMediator Mediator, RepoListCache cache, ILogger<RepoListCommandHandler> logger)
    : INotificationHandler<RepoListWasCreated>,
    INotificationHandler<GetRepoListCommand>
{
    public async Task Handle(GetRepoListCommand notification, CancellationToken cancellationToken)
    {
        try
        {
            List<Repository> repos;

            if (notification.force || cache.Read() == null)
            {
                repos = (await fetcher.GetRepositories())
                    .OrderBy(x => x.NamePrefix)
                    .ThenBy(x => x.VirtualPath)
                    .ThenBy(x => x.Name)
                    .ToList();
            }
            else
            {
                repos = cache.Read();
            }

            await Mediator.Publish(new RepoListWasCreated(repos));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while fetching repos");
        }
    }

    public Task Handle(RepoListWasCreated notification, CancellationToken cancellationToken)
    {
        Task.Run(() => engine.PullOrCloneItems(notification.Repos));
        return Task.CompletedTask;
    }
}
