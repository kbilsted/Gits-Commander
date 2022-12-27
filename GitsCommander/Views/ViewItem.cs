using GitsCommander.Gui;
using GitsCommander.RepoListAggregate;

namespace GitsCommander.Views;

public record ViewItem : WorkItemProjection, ISelectListItem
{
    public ViewItem(Repository Item, WorkItemProjectionState State, string StatusMessage) : base(Item, State, StatusMessage)
    {
    }

    public int Id => Item.Id;

    public ConsoleColor? Color { get => GetColor(State); set => throw new NotImplementedException(); }

    public override string ToString()
    {
        return join(Item.NamePrefix, join(Item.VirtualPath, Item.Name));
    }

    string join(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1))
            return s2;
        return $"{s1}/{s2}";
    }

    public ConsoleColor GetColor(WorkItemProjectionState state)
    {
        switch (state)
        {
            case WorkItemProjectionState.Ready:
                return ConsoleColor.DarkGray;
            case WorkItemProjectionState.Pulling:
                return ConsoleColor.White;
            case WorkItemProjectionState.LocalChanges:
                return ConsoleColor.Red;
            case WorkItemProjectionState.RemoteChanges:
                return ConsoleColor.Green;
            case WorkItemProjectionState.Done:
                return ConsoleColor.Cyan;
        };
        return ConsoleColor.DarkMagenta;
    }
}
