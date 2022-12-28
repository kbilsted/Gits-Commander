using GitsCommander.Gateways.GitHub;
using GitsCommander.Gateways.Gitlab;
using GitsCommander.Infrastructure;
using System.Text.RegularExpressions;

namespace GitsCommander.RepoListAggregate;

class RepositoriesLogic
{
    private int nextId = 0;
    private readonly GitlabClient gitLabClient;
    private readonly GitHubClient gitHubClient;
    private readonly IOptionsMonitor<Configuration> options;
    private readonly ILogger<RepositoriesLogic> logger;

    public RepositoriesLogic(
        GitlabClient gitLabClient,
        GitHubClient gitHubClient,
        IOptionsMonitor<Configuration> options,
        ILogger<RepositoriesLogic> logger)
    {
        this.gitLabClient = gitLabClient;
        this.gitHubClient = gitHubClient;
        this.options = options;
        this.logger = logger;
    }

    public async Task<IEnumerable<Repository>> GetRepositories()
    {
        List<Repository> result = new();

        if (options.CurrentValue.RepositorySources.GitLabs != null)
            await Gitlabs(result);

        if (options.CurrentValue.RepositorySources.GitHubs != null)
            await Githubs(result);

        return result;
    }

    async Task Githubs(List<Repository> result)
    {
        try
        {
            foreach (var source in options.CurrentValue.RepositorySources.GitHubs)
            {
                var folder = $"{source.Value.DestinationFolder}";
                if (string.IsNullOrEmpty(folder))
                    throw new Exception("appsettings destinationfolder cannot be empty");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                foreach (var item in await gitHubClient.FetchRepoList(source.Value))
                {
                    if (!source.Value.RepoNameRegexMatchRex.IsMatch(item.name))
                    {
                        logger.LogInformation($"Ignoring '{item.name}'");
                        continue;
                    }

                    var virtualPath = "";
                    result.Add(new Repository(nextId++, source.Key, virtualPath, item.name, item.clone_url, folder));
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, $"fetching github: {e.Message}");
        }
    }

    async Task Gitlabs(List<Repository> result)
    {
        try
        {
            foreach (var source in options.CurrentValue.RepositorySources.GitLabs)
            {
                foreach (var group in await gitLabClient.FetchGroupList(source.Value))
                {
                    if (!source.Value.RepoNameRegexMatchRex.IsMatch(group.web_url))
                    {
                        logger.LogInformation($"Ignoring '{group.web_url}'");
                        continue;
                    }

                    var virtualPath = Regex.Match(group.web_url, "^https://(.*)(\\.([^/.]*))/groups/(?<name>.*)").Groups["name"].Value;
                    var folder = $"{source.Value.DestinationFolder}{virtualPath}";
                    if (string.IsNullOrEmpty(folder))
                        throw new Exception("appsettings destinationfolder cannot be empty");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    IEnumerable<Repository> items = await GetGroupItems(source.Key, source.Value, folder, group.id, virtualPath);
                    result.AddRange(items);
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, $"fetching gitlab: {e.Message}");
        }
    }

    private async Task<IEnumerable<Repository>> GetGroupItems(string gitlabSourceName, GitLabSource source, string folder, int groupId, string virtualPath)
    {
        logger.LogInformation($"Handling group '{groupId}' in folder '{folder}'");
        var repos = await gitLabClient.FetchRepoList(source, groupId);

        return repos.Select(x => new Repository(nextId++, gitlabSourceName, virtualPath, x.name, x.http_url_to_repo, folder)).ToArray();
    }
}

