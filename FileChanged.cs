using Microsoft.Extensions.Logging;

namespace backblaze_directory_monitor
{
    internal class FileChanged : FileSystemWatcher
    {
        private readonly Queue<FileSystemEventArgs> Changes = new Queue<FileSystemEventArgs>();
        private readonly BackBlazeService BlazeService;
        private readonly ILogger<FileChanged> Logger;

        public FileChanged(BackBlazeService blaze, ILogger<FileChanged> logger)
            :base (@"C:\Users\pablo\Documents\Upload\music")
        {

            BlazeService = blaze;
            Logger = logger;

            IncludeSubdirectories = true;
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;

            EnableRaisingEvents = true;

            Created += FileChanged_Event;
            Changed += FileChanged_Event;
            Renamed += FileChanged_Event;
            // Deleted += FileChanged_Event;
        }

        private void FileChanged_Event(object sender, FileSystemEventArgs e)
        {
            // log immediate detection
            Logger.LogInformation($"IMMEDIATE change detected: {e.ChangeType}; {e.FullPath}");
            Changes.Enqueue(e);
        }

        public async Task ProcessQueue(CancellationToken tk)
        {
            Dictionary<string, int> retryCount;
            const string BUCKET_ID = "b498a0ff90ac26f18f9c0f1d";

            if (!Changes.Any())
            {
                return;
            }

            var auth = await BlazeService.Authorize();
            if (auth != null)
            {
                var uploadUrl = await BlazeService.GetUploadUrl(auth, BUCKET_ID);
                if (uploadUrl != null)
                {
                    retryCount = new Dictionary<string, int>();

                    while (Changes.Any())
                    {
                        var e = Changes.Dequeue();

                        // log attempting file
                        Logger.LogInformation($"Processing: {e.FullPath}");

                        try
                        {
                            await BlazeService.UploadFile(uploadUrl, e.FullPath);

                            // log success if no exception happened?
                            Logger.LogInformation($"Success: '{e.FullPath}' processed");
                        }
                        catch(IOException ex) when (ex.Message.Contains("cannot access") && ex.Message.Contains("another process"))
                        {
                            if (retryCount.TryGetValue(e.FullPath, out int count))
                            {
                                if (count < 5)
                                {
                                    retryCount[e.FullPath]++;
                                }
                                else if (count < 15) 
                                {
                                    // staggered delay
                                    await Task.Delay(TimeSpan.FromSeconds(count * 10));
                                    retryCount[e.FullPath]++;
                                }
                                else
                                {
                                    // skip to next file without re-queueing
                                    Logger.LogError($"Failure: 'CannotAccess'; '{e.FullPath}'");
                                    continue;
                                }
                            }
                            else
                            {
                                retryCount[e.FullPath] = 1;
                            }

                            // requeue at the end of the list for later attempt.
                            Changes.Enqueue(e);
                        }
                        // todo: handle upload failure; fetch new uploadUrl;

                        if (tk.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
