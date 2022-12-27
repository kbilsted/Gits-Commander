using GitsCommander.Gateways;

namespace GitsCommander.RepoListAggregate;

class WorkItemEngine
{
    private readonly GitClient git;
    private readonly ILogger<WorkItemEngine> logger;
    private readonly IMediator mediator;
    readonly bool MutateRepo = true;

    public WorkItemEngine(GitClient git, ILogger<WorkItemEngine> logger, IMediator mediator)
    {
        this.git = git;
        this.logger = logger;
        this.mediator = mediator;
    }

    public void PullOrCloneItems(IEnumerable<Repository> items)
    {
        ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 8 };
        Parallel.ForEach(items, options, item =>
        {
            try
            {
                mediator.Publish(new WorkItemWorkStarted(item.Id));

                PullOrCloneItem(item);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
            }
            mediator.Publish(new WorkItemWorkFinished(item.Id));

        });

        void PullOrCloneItem(Repository item)
        {
            var path = Path.Join(item.DestinationPath, item.Name);

            if (!Directory.Exists(path))
            {
                GitClone(item);
                return;
            }

            bool containsFiles = Directory.EnumerateFileSystemEntries(path).Any();
            if (!containsFiles)
            {
                Directory.CreateDirectory(path);
                GitClone(item);
                return;
            }

            if (MayPullFromRepo(item.Id, path))
            {
                if (MutateRepo && item.Name == "angular-search")
                    git.ResetBranchForTesting(path, "af366da6a51d8c23e99dcb790cc4186995785bad");

                var currentBranch = git.GetCurrentBranch(path);
                mediator.Publish(new PullingBranchStarted(item.Id, currentBranch));

                string result = git.Pull(path);
                if (result != "Already up to date.")
                    mediator.Publish(new RepoHasRemoteChanges(item.Id));
            }
        }
    }

    private void GitClone(Repository item)
    {
        git.Clone(item.DestinationPath, item.Name, item.Url);
    }

    private bool MayPullFromRepo(int id, string path)
    {
        if (git.HasLocalChanges(path))
        {
            mediator.Publish(new RepoHasLocalChanges(id));
            return false;
        }

        // Todo maybe later add branch rules
        //if (currentBranch != "master")
        //{
        //    return false;
        //}

        return true;
    }
}
