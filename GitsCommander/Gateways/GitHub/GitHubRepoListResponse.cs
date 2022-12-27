#nullable disable
namespace GitsCommander.Gateways.GitHub;

public class GitHubRepoListResponse
{
    public string name { get; set; }
    public string full_name { get; set; }
    public string clone_url { get; set; }
    public string ssh_url { get; set; }
}

