using GitsCommander.Infrastructure;

namespace GitsCommander.Gateways.GitHub;

public record GitHubClient(HttpClient Client, ILogger<GitHubClient> Logger)
{
    public async Task<GitHubRepoListResponse[]> FetchRepoList(GitHubSource source)
    {
        var l1 = await FetchRepoListApi(source);
        var l2 = await FetchRepoListForUser(source);

        var result = l1.Concat(l2.Where(x => !l1.Any(y => y.full_name == x.full_name))).ToArray();

        return result;
    }

    async Task<GitHubRepoListResponse[]> FetchRepoListApi(GitHubSource source)
    {
        int page = 0;
        var result = new List<GitHubRepoListResponse>();

        string cfgUrl = source.Connection.ReposApiUrl;
        if (!string.IsNullOrEmpty(cfgUrl))
        {
            while (await GetPage()) { }
        }

        return result.ToArray();

        async Task<bool> GetPage()
        {
            page++;
            string url = cfgUrl.Replace("{page}", $"{page}");
            Logger.LogInformation("url", url);
            var list = await Client.GetAsync<GitHubRepoListResponse[]>(x => Mutate(x, source), url);
            result.AddRange(list);
            return list.Any();
        }
    }

    async Task<GitHubRepoListResponse[]> FetchRepoListForUser(GitHubSource source)
    {
        int page = 0;
        var result = new List<GitHubRepoListResponse>();

        string cfgUrl = source.Connection.SearchApiUrl;
        if (!string.IsNullOrEmpty(cfgUrl))
        {
            while (await GetPage()) { }
        }

        return result.ToArray();

        async Task<bool> GetPage()
        {
            page++;
            string url = cfgUrl.Replace("{page}", $"{page}");
            Logger.LogInformation("url", url);
            var list = await Client.GetAsync<GithubUsersReposResponse>(x => Mutate(x, source), url);
            result.AddRange(list.items.Select(x => new GitHubRepoListResponse() { name = x.name, full_name = x.full_name, clone_url = x.clone_url, ssh_url = x.ssh_url }));
            return list.incomplete_results;
        }
    }

    private void Mutate(System.Net.Http.HttpClient client, GitHubSource source)
    {
        client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("GitsCommander", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", source.Connection.ApiKey);
    }
}
