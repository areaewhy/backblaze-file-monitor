using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

namespace backblaze_directory_monitor
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .UseWindowsService(
                    options => options.ServiceName = "Backblaze-Upload-Monitor")
                .ConfigureServices(
                (context, services) =>
                {
                    // windows only:
                    LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(services);

                    services.AddScoped<BackBlazeService>();
                    services.AddHostedService<WatcherService>();
                    services.AddSingleton<FileChanged>();



                    services.AddLogging(builder =>
                        builder.AddConfiguration(
                            context.Configuration.GetSection("Logging")));
                });


            var app = builder.Build();

            app.Run();
        }
    }
}