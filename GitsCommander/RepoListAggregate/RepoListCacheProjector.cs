namespace GitsCommander.RepoListAggregate;

public class RepoListCacheProjector : INotificationHandler<RepoListWasCreated>
{
    private readonly RepoListCache cache;

    public RepoListCacheProjector(RepoListCache cache)
    {
        this.cache = cache;
    }

    public async Task Handle(RepoListWasCreated notification, CancellationToken cancellationToken)
    {
        cache.Write(notification.Repos);
    }
}