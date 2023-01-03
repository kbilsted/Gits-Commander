using GitsCommander.Infrastructure;
using GitsCommander.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace GitsCommander;

class Program
{
    public static void Main(string[] args)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.secret.json", optional: true, reloadOnChange: true)
            .Build();

        // TODO look into nedsted logging

        var host = new HostBuilder()
            .ConfigureLogging((ctx, lb) =>
            {
                lb.AddConfiguration(config);
                lb.AddFileLogger();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddMediatR(Assembly.GetExecutingAssembly());

                services.AddSingleton<MainWindow, MainWindow>();

                typeof(Program).Assembly.GetTypes()
                    .Where(x => x.IsClass && !x.IsAbstract)
                    .Any(x => services.AddTransient(x) == null);

                services.AddHttpClient(); // IHttpClientFactory

                services.Configure<Configuration>(config.GetRequiredSection("Configuration"));
                services.AddOptions<Configuration>().ValidateDataAnnotations().ValidateOnStart();
            })
            .UseConsoleLifetime()
            .Build();

        using (var serviceScope = host.Services.CreateScope())
        {
            serviceScope.ServiceProvider.GetRequiredService<Startup>().Execute();
        }
    }
}

