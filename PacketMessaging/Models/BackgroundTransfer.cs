using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Web;
using MetroLog;

namespace PacketMessaging.Models
{
    public class BackgroundTransfer : IDisposable
    {
        private ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<BackgroundTransfer>();

        private Views.MainPage rootPage = Views.MainPage.Current;
        private List<DownloadOperation> activeDownloads;
        private CancellationTokenSource cts;

        private static Object singletonCreationLock = new Object();

        static BackgroundTransfer backgroundTransfer = null;

        private BackgroundTransfer()
        {
            cts = new CancellationTokenSource();
        }

        public static BackgroundTransfer CreateBackgroundTransfer()
        {
            if (backgroundTransfer == null)
            { 
                lock (singletonCreationLock)
                {
                    if (backgroundTransfer == null)
                    {
                        backgroundTransfer = new BackgroundTransfer();
                    }
                }
            }
            return backgroundTransfer;
        }

        //public void Dispose()
        //{
        //    if (cts != null)
        //    {
        //        cts.Dispose();
        //        cts = null;
        //    }

        //    GC.SuppressFinalize(this);
        //}

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        //protected async override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //    // An application must enumerate downloads when it gets started to prevent stale downloads/uploads.
        //    // Typically this can be done in the App class by overriding OnLaunched() and checking for
        //    // "args.Kind == ActivationKind.Launch" to detect an actual app launch.
        //    // We do it here in the sample to keep the sample code consolidated.
        //    await DiscoverActiveDownloadsAsync();
        //}


        // Enumerate the downloads that were going on in the background while the app was closed.

        public async Task DiscoverActiveDownloadsAsync()
        {
            activeDownloads = new List<DownloadOperation>();
            IReadOnlyList<DownloadOperation> downloads = null;
            try
            {
                downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            }
            catch (Exception ex)
            {
                if (!IsExceptionHandled("Discovery error", ex))
                {
                    throw;
                }
                return;
            }

            log.Info("Loading background downloads: " + downloads.Count);

            if (downloads.Count > 0)
            {
                List<Task> tasks = new List<Task>();
                foreach (DownloadOperation download in downloads)
                {
                    //Log(String.Format(CultureInfo.CurrentCulture,
                    //    "Discovered background download: {0}, Status: {1}", download.Guid,
                    //    download.Progress.Status));

                    // Attach progress and completion handlers.
                    tasks.Add(HandleDownloadAsync(download, false));
                }

                // Don't await HandleDownloadAsync() in the foreach loop since we would attach to the second
                // download only when the first one completed; attach to the third download when the second one
                // completes etc. We want to attach to all downloads immediately.
                // If there are actions that need to be taken once downloads complete, await tasks here, outside
                // the loop.
                await Task.WhenAll(tasks);
            }
        }

        public async void StartDownloadAsync(string serverAddress)
        {
            // Validating the URI is required since it was received from an untrusted source (user input).
            // The URI is validated by calling Uri.TryCreate() that will return 'false' for strings that are not valid URIs.
            // Note that when enabling the text box users may provide URIs to machines on the intrAnet that require
            // the "Home or Work Networking" capability.
            if (!Uri.TryCreate(serverAddress.Trim(), UriKind.Absolute, out Uri source))
            {
                rootPage.ShowMessageBox($"Invalid URI ({serverAddress.Trim()}).");
                return;
            }

            int index = serverAddress.LastIndexOf('/');
            string destination = serverAddress.Substring(index + 1);

            if (string.IsNullOrWhiteSpace(destination))
            {
                rootPage.ShowMessageBox("A local file name is required.");
                return;
            }

            StorageFile destinationFile;
            try
            {
                destinationFile = await Views.MainPage._archivedMessagesFolder.CreateFileAsync(destination, CreationCollisionOption.ReplaceExisting);
            }
            catch (FileNotFoundException ex)
            {
                rootPage.ShowMessageBox("Error while creating file: " + ex.Message);
                return;
            }

            BackgroundDownloader downloader = new BackgroundDownloader();
            DownloadOperation download = downloader.CreateDownload(source, destinationFile);

            log.Info(String.Format($"Downloading {source.AbsoluteUri} to {destinationFile.Name}, {download.Guid}"));

            download.Priority = BackgroundTransferPriority.Default;

            // Attach progress and completion handlers.
            await HandleDownloadAsync(download, true);
        }

        //private void StartDownload_Click(object sender, RoutedEventArgs e)
        //{
        //    StartDownload(BackgroundTransferPriority.Default);
        //}

        //private void PauseAll_Click(object sender, RoutedEventArgs e)
        //{
        //    Log("Downloads: " + activeDownloads.Count);

        //    foreach (DownloadOperation download in activeDownloads)
        //    {
        //        // DownloadOperation.Progress is updated in real-time while the operation is ongoing. Therefore,
        //        // we must make a local copy so that we can have a consistent view of that ever-changing state
        //        // throughout this method's lifetime.
        //        BackgroundDownloadProgress currentProgress = download.Progress;

        //        if (currentProgress.Status == BackgroundTransferStatus.Running)
        //        {
        //            download.Pause();
        //            Log("Paused: " + download.Guid);
        //        }
        //        else
        //        {
        //            Log(String.Format(CultureInfo.CurrentCulture, "Skipped: {0}, Status: {1}", download.Guid,
        //                currentProgress.Status));
        //        }
        //    }
        //}

        //private void ResumeAll_Click(object sender, RoutedEventArgs e)
        //{
        //    Log("Downloads: " + activeDownloads.Count);

        //    foreach (DownloadOperation download in activeDownloads)
        //    {
        //        // DownloadOperation.Progress is updated in real-time while the operation is ongoing. Therefore,
        //        // we must make a local copy so that we can have a consistent view of that ever-changing state
        //        // throughout this method's lifetime.
        //        BackgroundDownloadProgress currentProgress = download.Progress;

        //        if (currentProgress.Status == BackgroundTransferStatus.PausedByApplication)
        //        {
        //            download.Resume();
        //            Log("Resumed: " + download.Guid);
        //        }
        //        else
        //        {
        //            Log(String.Format(CultureInfo.CurrentCulture, "Skipped: {0}, Status: {1}", download.Guid,
        //                currentProgress.Status));
        //        }
        //    }
        //}

        //private void CancelAll_Click(object sender, RoutedEventArgs e)
        //{
        //    Log("Canceling Downloads: " + activeDownloads.Count);

        //    cts.Cancel();
        //    cts.Dispose();

        //    // Re-create the CancellationTokenSource and activeDownloads for future downloads.
        //    cts = new CancellationTokenSource();
        //    activeDownloads = new List<DownloadOperation>();
        //}

        // Note that this event is invoked on a background thread, so we cannot access the UI directly.
        //private void DownloadProgress(DownloadOperation download)
        //{
        //    // DownloadOperation.Progress is updated in real-time while the operation is ongoing. Therefore,
        //    // we must make a local copy so that we can have a consistent view of that ever-changing state
        //    // throughout this method's lifetime.
        //    BackgroundDownloadProgress currentProgress = download.Progress;

        //    MarshalLog(String.Format(CultureInfo.CurrentCulture, "Progress: {0}, Status: {1}", download.Guid,
        //        currentProgress.Status));

        //    double percent = 100;
        //    if (currentProgress.TotalBytesToReceive > 0)
        //    {
        //        percent = currentProgress.BytesReceived * 100 / currentProgress.TotalBytesToReceive;
        //    }

        //    MarshalLog(String.Format(
        //        CultureInfo.CurrentCulture,
        //        " - Transfered bytes: {0} of {1}, {2}%",
        //        currentProgress.BytesReceived,
        //        currentProgress.TotalBytesToReceive,
        //        percent));

        //    if (currentProgress.HasRestarted)
        //    {
        //        MarshalLog(" - Download restarted");
        //    }

        //    if (currentProgress.HasResponseChanged)
        //    {
        //        // We have received new response headers from the server.
        //        // Be aware that GetResponseInformation() returns null for non-HTTP transfers (e.g., FTP).
        //        ResponseInformation response = download.GetResponseInformation();
        //        int headersCount = response != null ? response.Headers.Count : 0;

        //        MarshalLog(" - Response updated; Header count: " + headersCount);

        //        // If you want to stream the response data this is a good time to start.
        //        // download.GetResultStreamAt(0);
        //    }
        //}

        private async Task HandleDownloadAsync(DownloadOperation download, bool start)
        {
            try
            {
                log.Info("Running: " + download.Guid);

                // Store the download so we can pause/resume.
                activeDownloads.Add(download);

                //Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                if (start)
                {
                    // Start the download and attach a progress handler.
                    await download.StartAsync().AsTask(cts.Token);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler.
                    await download.AttachAsync().AsTask(cts.Token/*, progressCallback*/);
                }

                //ResponseInformation response = download.GetResponseInformation();

                // GetResponseInformation() returns null for non-HTTP transfers (e.g., FTP).
                //string statusCode = response != null ? response.StatusCode.ToString() : String.Empty;

                //LogStatus(
                //    String.Format(
                //        CultureInfo.CurrentCulture,
                //        "Completed: {0}, Status Code: {1}",
                //        download.Guid,
                //        statusCode),
                //    NotifyType.StatusMessage);
            }
            catch (TaskCanceledException)
            {
                log.Info("Canceled: " + download.Guid);
            }
            catch (Exception ex)
            {
                if (!IsExceptionHandled("Execution error", ex, download))
                {
                    throw;
                }
            }
            finally
            {
                activeDownloads.Remove(download);
            }
        }

        private bool IsExceptionHandled(string title, Exception ex, DownloadOperation download = null)
        {
            WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
            if (error == WebErrorStatus.Unknown)
            {
                return false;
            }

            if (download == null)
            {
                rootPage.ShowMessageBox(String.Format($"Error: {title}: {error}"));
            }
            else
            {
                rootPage.ShowMessageBox(String.Format($"Error: {download.Guid} - {title}: {error}"));
            }

            return true;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BackgroundTransfer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        // When operations happen on a background thread we have to marshal UI updates back to the UI thread.
        //private void MarshalLog(string value)
        //{
        //    var ignore = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
        //    {
        //        Log(value);
        //    });
        //}

        //private void Log(string message)
        //{
        //    log.Info(message);
        //}

        //private void LogStatus(string message, NotifyType type)
        //{
        //    rootPage.NotifyUser(message, type);
        //    Log(message);
        //}

    }
}
