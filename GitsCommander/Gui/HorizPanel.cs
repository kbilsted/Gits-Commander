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
