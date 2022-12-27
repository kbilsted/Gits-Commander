using GitsCommander.Infrastructure;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GitsCommander.Gateways;

public class GitClient
{
    private readonly ILogger<GitClient> logger;
    private readonly string gitExePath;

    public GitClient(IOptionsMonitor<Configuration> options, ILogger<GitClient> logger)
    {
        this.logger = logger;
        gitExePath = options.CurrentValue.GitExePath;
    }

    private string Run(string gitexe, string args, string workingDirectory, StringBuilder? buffer = null, TimeSpan? timeout = null)
    {
        buffer ??= new StringBuilder();

        ProcessStartInfo startInfo = new ProcessStartInfo(gitexe, args)
        {
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
        };

        logger.LogInformation($"calling git '{args}'");

        var p = new Process();
        p.StartInfo = startInfo;
        p.EnableRaisingEvents = true;
        p.OutputDataReceived += new DataReceivedEventHandler((sender, evt) => buffer.AppendLine(evt.Data));

        // start the process
        // then begin asynchronously reading the output
        // then wait for the process to exit
        // then cancel asynchronously reading the output
        p.Start();
        p.BeginOutputReadLine();
        if (timeout.HasValue)
            p.WaitForExit(timeout.Value);
        else
            p.WaitForExit();

        p.CancelOutputRead();

        logger.LogInformation("Git exitCode: " + p.ExitCode);

        var output = buffer.ToString().Trim();
        logger.LogInformation("Done reading git output");


        if (p.ExitCode == 129)
            throw new Exception($"Wrong arguments to git: {output}");

        if (p.ExitCode == 0)
            return output;

        var exception = new Exception($"error during '{args}'. Output: {output}.");
        exception.Data.Add("ExitCode", p.ExitCode);
        throw exception;
    }

    public bool HasLocalChanges(string workingDirectory)
    {
        try
        {
            var result = Run(gitExePath, "status --porcelain", workingDirectory);
            return result != "";
        }
        catch (Exception e)
        {
            logger.LogError(e, "haslocalchanges");
            return true;
        }
    }

    private static readonly Regex GitStatusBranchExtract = new Regex("^On branch (?<name>.*)", RegexOptions.Multiline | RegexOptions.Compiled);

    public string GetCurrentBranch(string workingDirectory)
    {
        // git rev-parse --abbrev-ref HEAD 
        // works much nicer, but fails on empty repositories (there is no HEAD). git status works in all cases 
        var output = Run(gitExePath, "status", workingDirectory);

        var match = GitStatusBranchExtract.Match(output);
        if (match.Success)
            return match.Groups["name"].Value.Trim();

        throw new Exception($"{workingDirectory}: Cannot parse 'status' for output '{output}'");
    }

    public string Clone(string workingDirectory, string name, string url)
    {
        var arguments = $"clone -v --recurse-submodules --progress \"{url}\" \"{name}\"";
        return Run(gitExePath, arguments, workingDirectory);
    }

    public string Pull(string workingDirectory)
    {
        return Run(gitExePath, "pull --ff-only", workingDirectory);
    }

    public string ResetBranchForTesting(string workingDirectory, string commitId)
    {
        return Run(gitExePath, $"reset --hard {commitId}", workingDirectory);
    }
}