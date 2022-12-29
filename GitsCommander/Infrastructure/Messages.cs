namespace GitsCommander.Infrastructure;

public record ExceptionHasOccured(string Message) : INotification;
