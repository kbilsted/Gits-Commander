using GitsCommander.Infrastructure;
using GitsCommander.Views;

namespace GitsCommander.RepoListAggregate;

public class RepoListProjector :
    INotificationHandler<RepoListWasCreated>,
    INotificationHandler<RepoHasLocalChanges>,
    INotificationHandler<RepoHasRemoteChanges>,
    INotificationHandler<WorkItemWorkStarted>,
    INotificationHandler<WorkItemWorkFinished>,
    INotificationHandler<PullingBranchStarted>
{

    public Task Handle(RepoListWasCreated notification, CancellationToken cancellationToken)
    {
        var coll = notification.Repos.Select(x => new WorkItemProjection(x, WorkItemProjectionState.Ready, ""));

        InMemoryDatabase.RepoList.Projections.Clear();
        InMemoryDatabase.RepoList.Projections.AddRange(coll);
        InMemoryDatabase.RepoList.Outstanding = notification.Repos.Count;

        MainWindow.Window.PubSub(InMemoryDatabase.RepoList);
        return Task.CompletedTask;
    }

    public Task Handle(RepoHasLocalChanges notification, CancellationToken cancellationToken)
    {
        for (int i = 0; i < InMemoryDatabase.RepoList.Projections.Count; i++)
        {
            if (InMemoryDatabase.RepoList.Projections[i].Item.Id == notification.Id)
            {
                InMemoryDatabase.RepoList.Projections[i] = InMemoryDatabase.RepoList.Projections[i] with
                {
                    StatusMessage = "!! Local changes",
                    State = WorkItemProjectionState.LocalChanges
                };

                break;
            }
        };

        MainWindow.Window.PubSub(InMemoryDatabase.RepoList);
        return Task.CompletedTask;
    }

    public Task Handle(RepoHasRemoteChanges notification, CancellationToken cancellationToken)
    {
        for (int i = 0; i < InMemoryDatabase.RepoList.Projections.Count; i++)
        {
            if (InMemoryDatabase.RepoList.Projections[i].Item.Id == notification.Id)
            {
                InMemoryDatabase.RepoList.Projections[i] = InMemoryDatabase.RepoList.Projections[i] with
                {
                    StatusMessage = "Remote changes pulled",
                    State = WorkItemProjectionState.RemoteChanges
                };

                break;
            }
        };

        MainWindow.Window.PubSub(InMemoryDatabase.RepoList);
        return Task.CompletedTask;
    }

    public Task Handle(WorkItemWorkStarted notification, CancellationToken cancellationToken)
    {
        for (int i = 0; i < InMemoryDatabase.RepoList.Projections.Count; i++)
        {
            if (InMemoryDatabase.RepoList.Projections[i].Item.Id == notification.Id)
            {

                InMemoryDatabase.RepoList.Projections[i] = InMemoryDatabase.RepoList.Projections[i] with
                {
                    StatusMessage = "Pulling...",
                    State = WorkItemProjectionState.Pulling
                };

                break;
            }
        };

        MainWindow.Window.PubSub(InMemoryDatabase.RepoList);
        return Task.CompletedTask;
    }

    public Task Handle(WorkItemWorkFinished notification, CancellationToken cancellationToken)
    {
        InMemoryDatabase.RepoList.Outstanding--;

        MainWindow.Window.PubSub(InMemoryDatabase.RepoList);
        return Task.CompletedTask;
    }

    public Task Handle(PullingBranchStarted notification, CancellationToken cancellationToken)
    {
        for (int i = 0; i < InMemoryDatabase.RepoList.Projections.Count; i++)
        {
            if (InMemoryDatabase.RepoList.Projections[i].Item.Id == notification.Id)
            {
                InMemoryDatabase.RepoList.Projections[i] = InMemoryDatabase.RepoList.Projections[i] with
                {
                    StatusMessage = $"Pulling {notification.Branch} ...",
                    State = WorkItemProjectionState.Pulling
                };

                break;
            }
        };

        MainWindow.Window.PubSub(InMemoryDatabase.RepoList);
        return Task.CompletedTask;
    }
}
