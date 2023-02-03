using GitsCommander.RepoListAggregate;

namespace GitsCommander.Infrastructure;

static class InMemoryDatabase
{
    public static RepoListProjection RepoList { get; set; } = new RepoListProjection();
}
