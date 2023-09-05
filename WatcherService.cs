using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace backblaze_directory_monitor
{
    internal class WatcherService : BackgroundService
    {
        private readonly FileChanged ChangeMonitor;
        private readonly ILogger<WatcherService> Logger;

        public WatcherService(
          FileChanged changed,
          ILogger<WatcherService> logger)
        {
            ChangeMonitor = changed;
            Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await ChangeMonitor.ProcessQueue(stoppingToken);
                    await Task.Delay(TimeSpan.FromSeconds(15.0));
                }
            }
            catch (TaskCanceledException ex)
            {
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                Environment.Exit(1);
            }
        }
    }
}
