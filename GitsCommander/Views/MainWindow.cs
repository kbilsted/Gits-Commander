using GitsCommander.Gui;
using GitsCommander.Infrastructure;
using GitsCommander.LaunchAggregate;
using GitsCommander.RepoListAggregate;
using System.Collections.Immutable;

namespace GitsCommander.Views;

public class MainWindow : Window
{
    internal static MainWindow? Window;

    private readonly HorizPanel launchBar;
    private TextBox topBar { get; set; }
    private TextBox statusBar { get; set; }
    private SelectList<ViewItem> workList { get; set; }

    public MainWindow(IMediator mediator)
    {
        topBar = new TextBox(Console.WindowWidth, "Gits Commander v1.0 by Kasper B. Graversen", null)
        {
            BackgroundColor = ConsoleColor.DarkBlue
        };
        AddChild(topBar);

        workList = new SelectList<ViewItem>(Console.WindowHeight - 4, Console.WindowWidth, new List<ViewItem>());
        AddChild(workList);

        statusBar = new TextBox(Console.WindowWidth, "Waiting for repo list...", CalculateStatusBar)
        {
            BackgroundColor = ConsoleColor.DarkBlue
        };
        AddChild(statusBar);

        launchBar = new HorizPanel(0, 0, 1, Console.WindowWidth);
        AddChild(launchBar);

        if (Window == null)
        {
            Window = this;
            mediator.Publish(new GetRepoListCommand());
            mediator.Publish(new GetLaunchsettingsCommand());
        }

        base.keyboardcode = ch =>
        {
            if (ch.Key == ConsoleKey.F1
            || ch.Key == ConsoleKey.F2
            || ch.Key == ConsoleKey.F3
            || ch.Key == ConsoleKey.F4
            || ch.Key == ConsoleKey.F5
            || ch.Key == ConsoleKey.F6
            || ch.Key == ConsoleKey.F7
            || ch.Key == ConsoleKey.F8
            || ch.Key == ConsoleKey.F9
            || ch.Key == ConsoleKey.F10
            || ch.Key == ConsoleKey.F11
            || ch.Key == ConsoleKey.F12
            )
            {
                int? active = workList.GetActiveId();
                if (active != null)
                    mediator.Publish(new LaunchAProgramCommand(active.Value, ch.Key.ToString()));
            }
            if (ch.Key == ConsoleKey.UpArrow)
            {
                workList.Up();
            }
            if (ch.Key == ConsoleKey.DownArrow)
            {
                workList.Down();
            }
            if (ch.KeyChar == '1')
            {
                workList.Active = 10;
            }
            if (ch.KeyChar == '2')
            {
                workList.Active = 20;
            }
            if (ch.KeyChar == '3')
            {
                workList.Active = 30;
            }
            if (ch.KeyChar == 'd')
            {
                workList.RemoveItem(workList.Active);
            }
        };

        string CalculateStatusBar()
        {
            if (workList.Items.Any())
            {
                int id = workList.GetActiveId()!.Value;
                string status = data.Projections.SingleOrDefault(x => x.Item.Id == id)?.StatusMessage ?? "";
                return $"Work: {data.Outstanding} - Selected: {status}";
            }
            return "Waiting for repo list...";
        }
    }

    public Task Build()
    {
        return Task.Run(() => { HandleInput(); });
    }

    RepoListProjection data = new RepoListProjection();
    public void PubSub(RepoListProjection data)
    {
        lock (Lock)
        {
            this.data = data;

            var viewitems = data.Projections
                .Select(x => new ViewItem(x.Item, x.State, x.StatusMessage))
                .ToList();
            workList.UpdateItems(viewitems);
        }
    }

    public void PubSub(Dictionary<string, LaunchSetting> launchers)
    {
        lock (Lock)
        {
            launchBar.ClearChildren();

            var sortedLaunchers = launchers
                .Select(x => new { x, Sortkey = x.Key.PadLeft(4) })
                .OrderBy(x => x.Sortkey)
                .Select(x => x.x)
                .ToImmutableArray();

            for (int i = 0; i < sortedLaunchers.Length; i++)
            {
                var launcher = sortedLaunchers[i];

                var text = $"{launcher.Key}";

                var butn = new TextBox(text.Length, text, null)
                {
                    ForegroundColor = ConsoleColor.Black,
                    BackgroundColor = ConsoleColor.DarkGray
                };
                launchBar.AddChild(butn);


                if (i == sortedLaunchers.Length - 1)
                    text = $" {launcher.Value.Name}";
                else
                    text = $" {launcher.Value.Name}       ";
                TextBox butnText = new TextBox(text.Length, text, null)
                {
                    BackgroundColor = ConsoleColor.Black
                };
                launchBar.AddChild(butnText);

                NeedRedraw = true;
            }
        }
    }
}
