using System.Text.RegularExpressions;
using GitsCommander.Views;

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
            // maybe needed - test later...eg when implementing goto line
            //if (value >= FilteredItems.Count)
            //    value = FilteredItems.Count - 1;
            //if (value < 0)
            //    value = 0;

            while (value > active)
                Down();
            while (value < active)
                Up();
        }
    }

    int active;

    int topShowElemt;
    int bottonShowElemt;

    bool showActiveLine = true;

    public bool ShowActiveLine
    {
        get
        {
            return ShowActiveLine;
        }
        set
        {
            showActiveLine = value;
            NeedRedraw = true;
        }
    }

    public List<T> Items
    {
        get { return items; }
        set
        {
            UpdateItems(value);
        }
    }

    private List<T> items;

    public bool Filter(T item)
    {
        return NameFilterRex.IsMatch(item.ToString());
    }

    public List<T> FilteredItems;

    public bool ShowNumbers { get; set; } = true;
    private string? _nameFilter;

    public string NameFilter
    {
        get { return _nameFilter; }
        set
        {
            _nameFilter = value;
            NameFilterRex = MakeNameFilterRex(value);
            NeedRedraw = true;
        }
    }

    Regex NameFilterRex = new Regex(".");

    static Regex? MakeNameFilterRex(string nameFilter)
    {
        if (nameFilter == "")
            return new Regex(".");

        var regexOptions = nameFilter.All(char.IsLower) ? RegexOptions.IgnoreCase : RegexOptions.None;
        var pattern = string.Join("", nameFilter.Select(x =>
        {
            if (char.IsUpper(x))
                return (".*" + x);
            return "" + x;
        }));
        return new Regex(pattern, regexOptions);
    }

    public int? GetActiveId() => FilteredItems.Count == 0
        ? null
        : FilteredItems[active].Id;

    readonly string EmptyRow;

    public SelectList(int height, int width, List<T> items, bool visible = true)
        : base(0, 0, height, width, visible)
    {
        Active = 0;
        topShowElemt = 0;
        bottonShowElemt = Height - 1;

        UpdateItems(items);

        EmptyRow = "".PadLeft(width);
    }

    public void RemoveItem(int index)
    {
        T item = FilteredItems[index];
        items.Remove(item);
        if (active >= FilteredItems.Count)
            active = FilteredItems.Count - 1;

        NeedRedraw = true;
    }

    public void UpdateItems(List<T> items)
    {
        if (items == null) throw new ArgumentNullException();

        this.items = items;
        FilteredItems = items.Where(Filter).ToList();

        if (FilteredItems.Count < Active)
        {
            Active = 0;
            topShowElemt = 0;
            bottonShowElemt = Height - 1;
        }

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
        int max = Math.Max(Height, FilteredItems.Count);
        for (int paintRow = 0; paintRow < max; paintRow++)
        {
            (string s, ConsoleColor color)? x = DrawLine(paintRow);

            if (x == null)
                continue;

            var tmp = Console.ForegroundColor;
            Console.ForegroundColor = x.Value.color;
            Console.WriteLine(x.Value.s);
            Console.ForegroundColor = tmp;
        }
    }

    private (string, ConsoleColor)? DrawLine(int paintRow)
    {
        if (!FilteredItems.Any())
            return ("".PadRight(Width), ConsoleColor.Black);
        if (paintRow < topShowElemt)
            return null;
        if (paintRow > bottonShowElemt)
            return null;

        if (showActiveLine && paintRow == Active)
            Console.BackgroundColor = ConsoleColor.DarkYellow;
        else
            Console.BackgroundColor = ConsoleColor.Black;

        sb.Clear();

        ConsoleColor foregroundColor = ConsoleColor.Black;

        if (paintRow >= FilteredItems.Count)
        {
            sb.Append("~".PadRight(Width));
        }
        else
        {
            if (ShowNumbers)
                sb.Append($"{paintRow,4}. ");
            sb.Append(FilteredItems[paintRow].ToString());
            sb.Append("".PadRight(Width));
            foregroundColor = FilteredItems[paintRow].Color ?? ConsoleColor.Gray;
        }

        return (sb.ToString(0, Math.Min(Width, sb.Length)), foregroundColor);
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
        if (active == FilteredItems.Count - 1)
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
