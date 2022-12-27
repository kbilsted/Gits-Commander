namespace GitsCommander.Gui;

public class TextBox : GuiComponent
{
    public string Value
    {
        get { return value; }
        set
        {
            if (this.value != value)
            {
                this.value = value;
                NeedRedraw = true;
            }
        }
    }
    string value;
    readonly Func<string>? CalculateContent;

    public TextBox(int width, string value, Func<string>? calculateContent = null, bool visible = true)
        : base(0, 0, 1, width, visible)
    {
        Value = value;
        this.value = value;
        CalculateContent = calculateContent;
    }

    public override void Draw()
    {
        if (CalculateContent != null)
        {
            Value = CalculateContent();
        }

        string toPrint = Width < Value.Length ? Value.Substring(0, Width) : Value.PadRight(Width);

        Console.SetCursorPosition(X, Y);
        Console.Write(toPrint);
    }
}
