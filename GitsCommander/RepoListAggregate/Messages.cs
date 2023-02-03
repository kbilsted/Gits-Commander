namespace GitsCommander.RepoListAggregate;

public record GetRepoListCommand(bool force) : INotification;
public record RepoListWasCreated(List<Repository> Repos) : INotification;

public record RepoHasLocalChanges(int Id) : INotification;
public record RepoHasRemoteChanges(int Id) : INotification;
public record WorkItemWorkStarted(int Id) : INotification;
public record WorkItemWorkFinished(int Id) : INotification;
public record PullingBranchStarted(int Id, string Branch) : INotification;

