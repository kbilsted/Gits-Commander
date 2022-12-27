#nullable disable
namespace GitsCommander.Gateways.GitHub;

class GithubUsersReposResponse
{
    public int total_count { get; set; }
    public bool incomplete_results { get; set; }
    public GithubUsersReposItem[] items { get; set; }
}

class GithubUsersReposItem
{
    public int id { get; set; }
    public string node_id { get; set; }
    public string name { get; set; }
    public string full_name { get; set; }
    public bool _private { get; set; }
    public string url { get; set; }
    public string forks_url { get; set; }
    public string git_url { get; set; }
    public string ssh_url { get; set; }
    public string clone_url { get; set; }
    public bool archived { get; set; }
    public bool disabled { get; set; }
    public string visibility { get; set; }
}
