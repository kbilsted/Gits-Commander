namespace GitsCommander.Gui;

public abstract class GuiComponent
{
    public static bool NeedRedraw { get; set; } = true;
    public static readonly object Lock = new object();

    public int Height;
    public int Width;
    public int X;
    public int Y;
    public (int, int)? CursorAfterPaint;

    protected List<GuiComponent> Children = new List<GuiComponent>();

    public bool Visible = true;

    public ConsoleColor ForegroundColor = ConsoleColor.White;
    public ConsoleColor BackgroundColor = ConsoleColor.Black;

    public GuiComponent(int x, int y, int height = 1, int width = 1, bool visible = true)
    {
        Console.CursorVisible = false;

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

        if (Visible == false)
            return false;

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
                if (Visible)
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
                }

                NeedRedraw = false;
            }
        }
    }
}
