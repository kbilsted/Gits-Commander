namespace GitsCommander.Gui;

public class Window : GuiComponent
{
    protected Action<ConsoleKeyInfo> keyboardcode;

    public Window(int height = 1, int width = 1, bool visible = true)
        : this((ConsoleKeyInfo x) => { }, height, width, visible)
    {
    }

    public Window(Action<ConsoleKeyInfo> keyboardcode, int height = 1, int width = 1, bool visible = true)
        : base(0, 0, height, width, visible)
    {
        this.keyboardcode = keyboardcode;
    }

    public void HandleInput()
    {
        while (true)
        {
            Draw();

            if (Console.KeyAvailable)
            {
                NeedRedraw = true;
                ConsoleKeyInfo ch = Console.ReadKey(true);
                keyboardcode(ch);
            }
            else
            {
                Thread.Sleep(50);
                ShouldRedraw();
            }
        }
    }
}

