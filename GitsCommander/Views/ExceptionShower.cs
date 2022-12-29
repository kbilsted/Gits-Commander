using GitsCommander.Infrastructure;

namespace GitsCommander.Views;

class ExceptionShower : INotificationHandler<ExceptionHasOccured>
{
    public Task Handle(ExceptionHasOccured notification, CancellationToken cancellationToken)
    {
        MainWindow.Window.Visible = false;
        Console.SetCursorPosition(0, 0);
        Console.BackgroundColor = ConsoleColor.Red;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("+----------------------------------------------------------------------------+");
        Console.WriteLine("|                                                                            |");
        Console.WriteLine("|                                                                            |");
        Console.WriteLine("| " + notification.Message.PadRight("                                                                              ".Length));
        Console.WriteLine("|                                                                            |");
        Console.WriteLine("|                                                                            |");
        Console.WriteLine("|                                                                            |");
        Console.WriteLine("| perhaps make changes in 'appsettings.json' and refresh the ui              |");
        Console.WriteLine("|                                                                            |");
        Console.WriteLine("|                                                                            |");
        Console.WriteLine("| Press ENTER to continues...                                                |");
        Console.WriteLine("+----------------------------------------------------------------------------+");

        Console.ReadLine();
        MainWindow.Window.Visible = true;

        return Task.CompletedTask;
    }

}