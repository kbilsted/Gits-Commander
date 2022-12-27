namespace GitsCommander.RepoListAggregate;

public enum WorkItemProjectionState { Ready, Pulling, LocalChanges, RemoteChanges, Done }

public record WorkItemProjection(Repository Item, WorkItemProjectionState State, string StatusMessage);

public class RepoListProjection
{
    public List<WorkItemProjection> Projections = new List<WorkItemProjection>();
    public int Outstanding;
}
