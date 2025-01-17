using GitsCommander.Gui;
using GitsCommander.Infrastructure;
using GitsCommander.LaunchAggregate;
using GitsCommander.RepoListAggregate;
using System.Collections.Immutable;

namespace GitsCommander.Views;
/*
                                       ///                                      
                                   //////////                                   
                                 ///////////////                                
                              ////////////////////                              
                              ///////////////////////                           
                         /       //////////////////////                         
                       //////      ///////////////////////                      
                    ///////////       //////////////////////                    
                  ////////////////           //////////////////                 
               ////////////////////           ///////////////////               
             //////////////////////           //////////////////////            
          //////////////////////////            //////////////////////          
        ///////////////////////////////    /      ///////////////////////       
     //////////////////////////////////    ///       //////////////////////     
   ////////////////////////////////////    //////           //////////////////  
 //////////////////////////////////////    ////////           ./////////////////
///////////////////////////////////////    ////////            /////////////////
 //////////////////////////////////////    /////////           /////////////////
   ////////////////////////////////////    ///////////       /////////////////  
      /////////////////////////////////    ////////////////////////////////     
        ///////////////////////////////    //////////////////////////////       
           //////////////////////////       ./////////////////////////          
             //////////////////////           //////////////////////            
                ///////////////////            //////////////////               
                  //////////////////          /////////////////                 
                     /////////////////.    /////////////////                    
                       ///////////////////////////////////                      
                          /////////////////////////////                         
                            /////////////////////////                           
                               ///////////////////                              
                                 ///////////////                                
                                    /////////                       
                                       ///
*/
enum InputMode
{
    NormalMode, SearchMode, OperatorMode
}

public class MainWindow : Window
{
    internal static MainWindow? Window;

    private readonly HorizPanel launchBar;
    private TextBox topBar { get; set; }
    private TextBox statusBar { get; set; }
    private SelectList<ViewItem> workList { get; set; }
    private InputMode Mode = InputMode.NormalMode;

    public MainWindow(IMediator mediator)
    {
        topBar = new TextBox(Console.WindowWidth - 1, "Gits Commander v1.0 by Kasper B. Graversen", null)
        {
            BackgroundColor = ConsoleColor.DarkBlue
        };
        AddChild(topBar);

        workList = new SelectList<ViewItem>(Console.WindowHeight - 4, Console.WindowWidth, new List<ViewItem>());
        AddChild(workList);

        statusBar = new TextBox(Console.WindowWidth - 1, "Waiting for repo list...", CalculateStatusBar)
        {
            BackgroundColor = ConsoleColor.DarkBlue
        };
        AddChild(statusBar);

        launchBar = new HorizPanel(0, 0, 1, Console.WindowWidth - 1);
        AddChild(launchBar);

        if (Window == null)
        {
            Window = this;
            mediator.Publish(new GetRepoListCommand(false));
            mediator.Publish(new GetLaunchsettingsCommand());
        }

        base.keyboardcode = ch => { KeyHandler(mediator, ch); };

        string CalculateStatusBar()
        {
            if (workList.FilteredItems.Any())
            {
                int id = workList.GetActiveId()!.Value;
                string status = data.Projections
                    .ToArray()//ensure enumeration despite other threads mutate collection
                    .SingleOrDefault(x => x.Item.Id == id)?.StatusMessage ?? "";
                return $"Work: {data.Outstanding,-5} - Selected: {status,-25} {workList.NameFilter}";
            }
            else
            {
                return $"Work: {data.Outstanding,-5} - Selected: {"",-25}  {workList.NameFilter}";

            }
            return "Waiting for repo list...";
        }
    }

    private void KeyHandler(IMediator mediator, ConsoleKeyInfo ch)
    {
        switch (Mode)
        {
            case InputMode.NormalMode:
                KeyHandlerNormalMode(mediator, ch);
                break;
            case InputMode.SearchMode:
                KeyHandlerSearchMode(ch);
                return;

            case InputMode.OperatorMode:
                // eg goto line
                throw new NotImplementedException("");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void KeyHandlerSearchMode(ConsoleKeyInfo ch)
    {
        if (ch.Key == ConsoleKey.Backspace)
        {
            if (workList.NameFilter == "")
                return;
            if (ch.Modifiers == ConsoleModifiers.Control)
            {
                workList.NameFilter = "";
                return;
            }
            workList.NameFilter = workList.NameFilter[0..^1];
            return;
        }

        if (ch.Key is ConsoleKey.Enter or ConsoleKey.Tab)
        {
            GoToNormalMode();
            return;
        }

        if (ch.Key is ConsoleKey.Escape)
        {
            workList.NameFilter = "";
            GoToNormalMode();
            return;
        }

        workList.NameFilter += ch.KeyChar;
                
        return;
    }

    void GoToNormalMode()
    {
        workList.ShowActiveLine = true;
        Mode = InputMode.NormalMode;
    }

    void GoToSearchMode()
    {
        workList.ShowActiveLine =false;
        Mode = InputMode.SearchMode;
    }

    private void KeyHandlerNormalMode(IMediator mediator, ConsoleKeyInfo ch)
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
        if (ch.Key is ConsoleKey.UpArrow or ConsoleKey.K)
        {
            workList.Up();
        }
        if (ch.Key is ConsoleKey.DownArrow or ConsoleKey.J)
        {
            workList.Down();
        }
        if (ch.Key is ConsoleKey.PageDown || ch.KeyChar is 'æ' or ';')
        {
            for (int i = 0; i < Console.WindowHeight / 3; i++)
                workList.Down();
        }
        if (ch.Key is ConsoleKey.PageUp or ConsoleKey.H)
        {
            for (int i = 0; i < Console.WindowHeight / 3; i++)
                workList.Up();
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
            // TODO implement delete folder
            workList.RemoveItem(workList.Active);
        }

        if (ch.KeyChar == '/' || ch.KeyChar == '-')
        {
            GoToSearchMode();
        }
        
        
    }

    public Task Build()
    {
        return Task.Run(() => { HandleInput(); });
    }

    public override void Draw()
    {
        base.Draw();

        if (Mode == InputMode.SearchMode && statusBar.CursorAfterPaint != null)
        {
            Console.CursorVisible = true;
            Console.CursorSize = 100;
            Console.SetCursorPosition(statusBar.CursorAfterPaint.Value.Item1, statusBar.CursorAfterPaint.Value.Item2);
        }
        else
        {
            Console.CursorVisible = false;
        }
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
