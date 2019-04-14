using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FortniteReplayWatcher.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(loggerFactory => loggerFactory.AddEventLog())
                .UseServiceBaseLifetime()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddMemoryCache();
                    services.AddHostedService<Worker>();
                });
        }
    }
}
