#nullable disable
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GitsCommander.Infrastructure;

public class Configuration
{
    [Required]
    public string GitExePath { get; set; }

    [Required]
    public Dictionary<string, LaunchSetting> Launchers { get; set; }
    
    [Required]
    public RepositorySourcesSettings RepositorySources { get; set; }
}

public class RepositorySourcesSettings
{
    public Dictionary<string, GitLabSource> GitLabs { get; set; }
    public Dictionary<string, GitHubSource> GitHubs { get; set; }
}

public class LaunchSetting
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Command { get; set; }
    
    public string[] Arguments { get; set; }
    public string FilePattern { get; set; }
}

public abstract class Source
{
    [Required]
    public string RepoNameRegexMatch { get; set; }

    Regex nameMatchRegex;
    public Regex RepoNameRegexMatchRex
    {
        get
        {
            return nameMatchRegex ??= new Regex(RepoNameRegexMatch ?? ".*", RegexOptions.Compiled);
        }
    }

    [Required]
    public string DestinationFolder { get; set; }
}

public class GitHubSource : Source
{
    [Required]
    public ConnectionInfo Connection { get; set; }

    public class ConnectionInfo
    {
        [Required]
        public string ApiKey { get; set; }
        [Required]
        public string ReposApiUrl { get; set; }
        [Required]
        public string SearchApiUrl { get; set; }
    }
}

public class GitLabSource : Source
{
    [Required]
    public GitlabConnectionInfo Connection { get; set; }

   
}
public class GitlabConnectionInfo
{
    [Required]
    public string ApiKey { get; set; }
    [Required]
    public string GroupsApiUrl { get; set; }
    [Required]
    public string ReposApiUrl { get; set; }
}