namespace GitsCommander.Gui;

public class HorizPanel : GuiComponent
{
    public HorizPanel(int x, int y, int height = 1, int width = 1, bool visible = true) : base(x, y, height, width, visible)
    {
    }

    public override GuiComponent AddChild(GuiComponent c)
    {
        c.X = Children.Any()
            ? Children.Last().X + Children.Last().Width
            : 0;
        c.Y = Y;
        Children.Add(c);
        return this;
    }
}

public abstract class GuiComponent
{
    public static bool NeedRedraw { get; set; } = true;
    public static object Lock = new object();

    public int Height;
    public int Width;
    public int X;
    public int Y;

    protected List<GuiComponent> Children = new List<GuiComponent>();

    public readonly bool Visible = true;

    public ConsoleColor ForegroundColor = ConsoleColor.White;
    public ConsoleColor BackgroundColor = ConsoleColor.Black;

    public GuiComponent(int x, int y, int height = 1, int width = 1, bool visible = true)
    {
        X = x;
        Y = y;
        Visible = visible;
        Height = height;
        Width = width;
    }

    public virtual bool ShouldRedraw()
    {
        if (NeedRedraw)
            return true;

        foreach (var item in Children)
        {
            if (item.ShouldRedraw())
            {
                NeedRedraw = true;
                return true;
            }
        }
        return false;
    }
    public virtual void ClearChildren() => Children.Clear();

    public virtual GuiComponent AddChild(GuiComponent c)
    {
        c.Y = Children.Any() ? Children.Last().Y + Children.Last().Height : 0;
        Children.Add(c);
        return this;
    }

    public virtual void Draw()
    {
        if (NeedRedraw)
        {
            lock (Lock)
            {
                Children
                    .Where(x => x.Visible)
                    .ToList()
                    .ForEach(x =>
                    {
                        Console.SetCursorPosition(x.X, x.Y);
                        Console.ForegroundColor = x.ForegroundColor;
                        Console.BackgroundColor = x.BackgroundColor;

                        x.Draw();
                    });

                NeedRedraw = false;
            }
        }
    }
}
