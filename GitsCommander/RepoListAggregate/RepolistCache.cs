using GitsCommander.Infrastructure;

namespace GitsCommander.RepoListAggregate;

public record RepoListCache(IOptionsMonitor<Configuration> Configuration)
{
    private List<Repository> cache;

    public List<Repository>? Read()
    {
        if (cache == null && File.Exists(Configuration.CurrentValue.RepoListLocalCachePath))
        {
            var json = File.ReadAllText(Configuration.CurrentValue.RepoListLocalCachePath);
            cache = JsonSerializer.Deserialize<List<Repository>>(json);
        }

        return cache;
    }

    public void Write(List<Repository> repos)
    {
        cache = repos;

        var options = new JsonSerializerOptions() { WriteIndented = true };
        var json = JsonSerializer.Serialize(repos, options);
        File.WriteAllTextAsync(Configuration.CurrentValue.RepoListLocalCachePath, json);
    }
}