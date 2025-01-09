using GitsCommander.Infrastructure;
using GitsCommander.LaunchAggregate;
using GitsCommander.RepoListAggregate;
using GitsCommander.Views;

namespace GitsCommander;

record Startup(
    IOptionsMonitor<Configuration> options,
    WorkItemEngine engine,
    ILogger<Startup> logger,
    IMediator mediator)
{

    public void Execute()
    {
        Console.Clear();

        logger.LogInformation("Starting application");

        var gui = new MainWindow(mediator);
        var task = gui.Build();

        options.OnChange(config => mediator.Publish(new LaunchSetttingsChanged()));

        Task.WaitAll(task);

        logger.LogInformation("Stopping application");
    }
}
