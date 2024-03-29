﻿using GitsCommander.Gateways.GitHub;
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
    private readonly IMediator mediator;

    public RepositoriesLogic(
        GitlabClient gitLabClient,
        GitHubClient gitHubClient,
        IOptionsMonitor<Configuration> options,
        ILogger<RepositoriesLogic> logger,
        IMediator mediator)
    {
        this.gitLabClient = gitLabClient;
        this.gitHubClient = gitHubClient;
        this.options = options;
        this.logger = logger;
        this.mediator = mediator;
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
                    if (!source.Value.RepoGroupPathRegexMatchRex.IsMatch(item.name))
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
            await mediator.Publish(new ExceptionHasOccured(e.Message));
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
                    var virtualPath = Regex.Match(group.web_url, "^https://(.*)(\\.([^/.]*))/groups/(?<name>.*)").Groups["name"].Value;

                    if (!source.Value.RepoGroupPathRegexMatchRex.IsMatch(virtualPath))
                    {
                        logger.LogInformation($"Ignoring '{group.web_url}'");
                        continue;
                    }

                    var prefixGobble = source.Value.DestinationPrefixGobble;
                    if (prefixGobble != null)
                    {
                        if (virtualPath.Length >= prefixGobble.Length && virtualPath.StartsWith(prefixGobble))
                            virtualPath = virtualPath.Replace(prefixGobble, "");
                        else if (virtualPath.Length < prefixGobble.Length && prefixGobble.StartsWith(virtualPath))
                            virtualPath = "";
                    }

                    var folder = Path.Combine(source.Value.DestinationFolder, virtualPath);

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
            logger.LogError(e, $"fetching gitlab: {e.Message}\n{e.StackTrace}");
            await mediator.Publish(new ExceptionHasOccured(e.Message));
        }
    }

    private async Task<IEnumerable<Repository>> GetGroupItems(string gitlabSourceName, GitLabSource source, string folder, int groupId, string virtualPath)
    {
        logger.LogInformation($"Handling group '{groupId}' in folder '{folder}'");
        var repos = await gitLabClient.FetchRepoList(source, groupId);

        return repos.Select(x => new Repository(nextId++, gitlabSourceName, virtualPath, x.name, x.http_url_to_repo, folder)).ToArray();
    }
}

