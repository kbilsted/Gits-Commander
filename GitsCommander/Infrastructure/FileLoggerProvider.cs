using System.Collections.Concurrent;

namespace GitsCommander.Infrastructure
{
    public static class FileLoggerProviderExt
    {
        public static void AddFileLogger(this ILoggingBuilder b)
        {
            b.AddProvider(new FileLoggerProvider());
        }
    }

    internal class FileLoggerProvider : ILoggerProvider
    {
        static readonly Logger log = new Logger();

        public ILogger CreateLogger(string categoryName)
        {
            return log;
        }

        public void Dispose()
        {
        }

        class Logger : ILogger
        {
            readonly ConcurrentQueue<string> messages = new ConcurrentQueue<string>();

            public Logger()
            {
                Task.Run(() =>
                {
                    List<string> list = new List<string>(200);
                    while (true)
                    {
                        Thread.Sleep(300);

                        list.Clear();

                        for (int i = 0; i < 50; i++)
                        {
                            if (messages.TryDequeue(out string? res) && res != null)
                                list.Add(res);
                            else
                                break;
                        }

                        File.AppendAllLines("GitsCommander.log", list);
                    }
                });
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                string msg = exception == null
                    ? $"{DateTime.Now.ToString("HH:mm:ss")} [{logLevel}] {state} {exception?.Message}"
                    : $"{DateTime.Now.ToString("HH:mm:ss")} [{logLevel}] {state} {exception?.Message}\n{exception?.StackTrace}";

                messages.Enqueue(msg);
            }
        }
    }
}