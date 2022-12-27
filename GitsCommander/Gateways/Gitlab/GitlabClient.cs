using GitsCommander.Infrastructure;

namespace GitsCommander.Gateways.Gitlab;

record GitlabClient(HttpClient client)
{
    public async Task<GitlabGroupListResponse[]> FetchGroupList(GitLabSource source)
    {
        var list = await client.GetAsync<GitlabGroupListResponse[]>(x => Mutate(x, source), source.Connection.GroupsApiUrl);
        return list;
    }

    private void Mutate(System.Net.Http.HttpClient client, GitLabSource source)
    {
        client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", source.Connection.ApiKey);
    }

    public async Task<GitlabRepoListResponse[]> FetchRepoList(GitLabSource source, int groupId)
    {
        var uri = source.Connection.ReposApiUrl.Replace("{groupId}", groupId.ToString());
        var repos = await client.GetAsync<GitlabRepoListResponse[]>(x => Mutate(x, source), uri);

        return repos.OrderBy(x => x.name).ToArray();
    }
}
