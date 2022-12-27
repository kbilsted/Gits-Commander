#nullable disable
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GitsCommander.Infrastructure;

public class Configuration
{
    [Required]
    public string GitExePath { get; set; }
    public Dictionary<string, LaunchSetting> Launchers { get; set; }
    public RepositorySourcesSettings RepositorySources { get; set; }
}

public class RepositorySourcesSettings
{
    public Dictionary<string, GitLabSource> GitLabs { get; set; }
    public Dictionary<string, GitHubSource> GitHubs { get; set; }
}

public class LaunchSetting
{
    public string Name { get; set; }
    public string Command { get; set; }
    public string[] Arguments { get; set; }
    public string Folder { get; set; }
    public string FilePattern { get; set; }
}

public abstract class Source
{
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
    public ConnectionInfo Connection { get; set; }

    public class ConnectionInfo
    {
        public string ApiKey { get; set; }
        public string ReposApiUrl { get; set; }
        public string SearchApiUrl { get; set; }
    }
}

public class GitLabSource : Source
{
    public ConnectionInfo Connection { get; set; }

    public class ConnectionInfo
    {
        public string ApiKey { get; set; }
        public string GroupsApiUrl { get; set; }
        public string ReposApiUrl { get; set; }
    }
}
