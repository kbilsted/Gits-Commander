using GitsCommander.Infrastructure;
using GitsCommander.RepoListAggregate;
using GitsCommander.Views;
using System.Diagnostics;

namespace GitsCommander.LaunchAggregate;

record LaunchCommandHandler(IMediator Mediator, IOptionsMonitor<Configuration> options)
    : INotificationHandler<LaunchAProgramCommand>,
    INotificationHandler<LaunchSetttingsChanged>,
    INotificationHandler<GetLaunchsettingsCommand>
{

    public Task Handle(LaunchAProgramCommand notification, CancellationToken cancellationToken)
    {
        LaunchSetting launcher;
        try
        {
            var item = InMemoryDatabase.RepoList.Projections.FirstOrDefault(x => x.Item.Id == notification.id);
            if (item == null)
            {
                Mediator.Publish(new LaunchProgramFailed($"Cannot find id '{notification.id}'"));
                return Task.CompletedTask;
            }

            if (!options.CurrentValue.Launchers.TryGetValue(notification.keyPressed, out launcher!))
            {
                Mediator.Publish(new LaunchProgramFailed($"No launcher configures in appsettings.json for key press '{notification.keyPressed}'."));
                return Task.CompletedTask;
            }
            if (launcher.Command == null)
                return Task.CompletedTask;

            HandleCommand(launcher, item);

            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            Mediator.Publish(new LaunchProgramFailed(e.Message)); // TODO listen for event
        }

        return Task.CompletedTask;
    }

    public Task Handle(LaunchSetttingsChanged notification, CancellationToken cancellationToken)
    {
        MainWindow.Window.PubSub(options.CurrentValue.Launchers);
        return Task.CompletedTask;
    }

    public Task Handle(GetLaunchsettingsCommand notification, CancellationToken cancellationToken)
    {
        Mediator.Publish(new LaunchSetttingsChanged());
        return Task.CompletedTask;
    }

    private void HandleCommand(LaunchSetting launcher, WorkItemProjection? item)
    {
        switch (launcher.Command)
        {
            case LauncherLanguage.SpecialRefreshRepo:
                Mediator.Publish(new GetRepoListCommand());
                return;

            case LauncherLanguage.OpenAppSettings:
                {
                    var pp = new Process
                    {
                        StartInfo = new ProcessStartInfo("appsettings.json")
                        {
                            CreateNoWindow = true,
                            WorkingDirectory = Directory.GetParent(AppContext.BaseDirectory)?.FullName,
                            UseShellExecute = true,
                        }
                    };
                    pp.Start();
                }
                return;
        }

        if (item == null)
            throw new Exception("Current selected item is null");

        var folder = Path.Join(item.Item.DestinationPath, item.Item.Name);

        var args = launcher.Arguments == null
            ? null
            : string.Join(" ", launcher.Arguments.Select(x => ReplaceMagicStrings(x, folder, launcher)));

        string exeFile = ReplaceMagicStrings(launcher.Command, folder, launcher);

        var p = new Process
        {
            StartInfo = new ProcessStartInfo(exeFile)
            {
                Arguments = args,
                CreateNoWindow = true,
                WorkingDirectory = folder,
                UseShellExecute = true,
            }
        };
        p.Start();
    }

    string ReplaceMagicStrings(string input, string folder, LaunchSetting launcher)
    {
        switch (input)
        {
            case LauncherLanguage.SpecialFile:
                string? file = Directory.EnumerateFiles(folder, launcher.FilePattern, SearchOption.AllDirectories).FirstOrDefault();
                if (file == null)
                    throw new Exception($"No '{launcher.FilePattern}' found'");
                return file;

            case LauncherLanguage.SpecialFolder:
                return folder;

            default:
                return input;
        }
    }
}
