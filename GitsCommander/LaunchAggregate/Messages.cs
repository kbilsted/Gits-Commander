namespace GitsCommander.LaunchAggregate;

public record GetLaunchsettingsCommand : INotification;
public record LaunchSetttingsChanged() : INotification;
public record LaunchAProgramCommand(int id, string keyPressed) : INotification;
public record LaunchProgramFailed(string error) : INotification;