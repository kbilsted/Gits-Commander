namespace GitsCommander.RepoListAggregate;

public record Repository(int Id, string NamePrefix, string VirtualPath, string Name, string Url, string DestinationPath);

