namespace GitsCommander.Gui;

public interface ISelectListItem
{
    int Id { get; }
    ConsoleColor? Color { get; set; }
}

public class SelectList<T> : GuiComponent
    where T : ISelectListItem
{

    public int Active
    {
        get { return active; }
        set
        {
            while (value > active)
                Down();
            while (value < active)
                Up();
        }
    }
    int active;

    int topShowElemt;
    int bottonShowElemt;

    public List<T> Items { get { return items; } set { UpdateItems(value); } }
    private List<T> items;
    public bool ShowNumbers { get; set; } = true;

    public int? GetActiveId() => items.Count == 0
        ? null
        : items[active].Id;

    readonly string EmptyRow;

    public SelectList(int height, int width, List<T> items, bool visible = true)
        : base(0, 0, height, width, visible)
    {
        UpdateItems(items);
        this.items = items;

        EmptyRow = "".PadLeft(width);
    }

    public void RemoveItem(int index)
    {
        Items.RemoveAt(index);
        if (active >= items.Count)
            active = items.Count - 1;

        NeedRedraw = true;
    }

    public void UpdateItems(List<T> items)
    {
        if (items.Count < Active || this.items == null)
        {
            Active = 0;
            topShowElemt = 0;
            bottonShowElemt = Height - 1;
        }
        this.items = items;
        NeedRedraw = true;
    }

    public void UpdateColor(int id, ConsoleColor color)
    {
        var pos = items.Single(x => x.Id == id).Color = color;
        NeedRedraw = true;
    }

    readonly StringBuilder sb = new StringBuilder();
    public override void Draw()
    {
        (string, ConsoleColor)? DrawI(int paintRow)
        {
            if (!items.Any())
                return ("".PadRight(Width), ConsoleColor.Black);
            if (paintRow < topShowElemt)
                return null;
            if (paintRow > bottonShowElemt)
                return null;

            if (paintRow == Active)
                Console.BackgroundColor = ConsoleColor.DarkYellow;
            else
                Console.BackgroundColor = ConsoleColor.Black;

            sb.Clear();

            ConsoleColor foregroundColor = ConsoleColor.Black;

            if (paintRow >= items.Count)
            {
                sb.Append("".PadRight(Width));
            }
            else
            {
                if (ShowNumbers)
                    sb.Append($"{paintRow,4}. ");
                sb.Append(items[paintRow].ToString());
                sb.Append("".PadRight(Width));
                foregroundColor = items[paintRow].Color ?? ConsoleColor.Gray;
            }

            return (sb.ToString(0, Math.Min(Width, sb.Length)), foregroundColor);
        }

        int max = Math.Max(Height, items.Count);
        for (int paintRow = 0; paintRow < max; paintRow++)
        {
            (string s, ConsoleColor color)? x = DrawI(paintRow);

            if (x == null)
                continue;

            var tmp = Console.ForegroundColor;
            Console.ForegroundColor = x.Value.color;
            Console.WriteLine(x.Value.s);
            Console.ForegroundColor = tmp;
        }
    }

    public void Up()
    {
        if (active == 0)
            return;

        active--;

        if (active < topShowElemt)
        {
            bottonShowElemt--;
            topShowElemt--;
        }

        NeedRedraw = true;
    }

    public void Down()
    {
        if (active == items.Count - 1)
            return;

        active++;

        if (active > bottonShowElemt)
        {
            bottonShowElemt++;
            topShowElemt++;
        }

        NeedRedraw = true;
    }
}
