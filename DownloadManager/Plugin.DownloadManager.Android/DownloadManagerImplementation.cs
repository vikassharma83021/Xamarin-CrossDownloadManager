using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Database;
using Android.Content;
using Plugin.DownloadManager.Abstractions;

namespace Plugin.DownloadManager
{
    public class DownloadManagerImplementation : IDownloadManager
    {
        Android.OS.Handler _downloadWatcherHandler;
        Java.Lang.Runnable _downloadWatcherHandlerRunnable;

        Android.App.DownloadManager _downloadManager;

        public ObservableCollection<IDownloadFile> Queue { get; private set; }

        public Func<IDownloadFile, string> UriForDownloadedFile { get; set; }

        public DownloadManagerImplementation (Context applicationContext)
        {
            Queue = new ObservableCollection<IDownloadFile> ();

            _downloadManager = (Android.App.DownloadManager)applicationContext.GetSystemService (Context.DownloadService);

            // Add all items to the Queue that are pending, paused or running
            LoopOnDownloads (new Action<ICursor> (cursor => ReinitializeFile (cursor)));

            // Check sequentially if parameters for any of the registered downloads changed
            StartDownloadWatcher ();
        }

        public IDownloadFile CreateDownloadFile (string url, IDictionary<string, string> headers)
        {
            return new DownloadFileImplementation (url, headers);
        }

        public void Start (IDownloadFile i)
        {
            var file = (DownloadFileImplementation)i;

            file.StartDownload (_downloadManager, UriForDownloadedFile (file));
            Queue.Add (file);
        }

        public void Abort (IDownloadFile i)
        {
            var file = (DownloadFileImplementation)i;

            file.Status = DownloadFileStatus.CANCELED;
            _downloadManager.Remove (file.Id);
            Queue.Remove (file);
        }

        public void AbortAll ()
        {
            foreach (var file in Queue.ToList ()) {
                Abort (file);
            }
        }

        void LoopOnDownloads (Action<ICursor> runnable)
        {
            // Reinitialize downloads that were started before the app was terminated or suspended
            var query = new Android.App.DownloadManager.Query ();
            query.SetFilterByStatus (
                Android.App.DownloadStatus.Paused |
                Android.App.DownloadStatus.Pending |
                Android.App.DownloadStatus.Running
            );

            try {
                using (var cursor = _downloadManager.InvokeQuery (query)) {
                    while (cursor.MoveToNext ()) {
                        runnable.Invoke (cursor);
                    }
                }
            } catch (Android.Database.Sqlite.SQLiteException) {
                // I lately got an exception that the database was unaccessible ...
            }
        }

        void ReinitializeFile (ICursor cursor)
        {
            var downloadFile = new DownloadFileImplementation (cursor);

            Queue.Add (downloadFile);
            UpdateFileProperties (cursor, downloadFile);
        }

        void StartDownloadWatcher ()
        {
            // Create an instance for a runnable-handler
            _downloadWatcherHandler = new Android.OS.Handler ();

            // Create a runnable, restarting itself to update every file in the queue
            _downloadWatcherHandlerRunnable = new Java.Lang.Runnable (() => {

                // Loop throught all files in the system-queue and update the data in the local queue
                LoopOnDownloads (cursor => UpdateFileProperties (cursor));

                var nextExecution = (Queue.Count == 0) ? 1000 : 100;
                _downloadWatcherHandler.PostDelayed (_downloadWatcherHandlerRunnable, nextExecution);
            });

            // Start this playing handler immediately
            _downloadWatcherHandler.PostDelayed (_downloadWatcherHandlerRunnable, 0);
        }

        public void UpdateFileProperties (ICursor cursor)
        {
            int id = cursor.GetInt (cursor.GetColumnIndex (Android.App.DownloadManager.ColumnId));
            var downloadFile = Queue.Cast<DownloadFileImplementation> ().FirstOrDefault (f => f.Id == id);

            if (downloadFile != null) {
                UpdateFileProperties (cursor, downloadFile);
            }
        }

        /**
         * Update the properties for a file by it's cursor.
         * This method should be called in an interval and on reinitialization.
         */
        public void UpdateFileProperties (ICursor cursor, DownloadFileImplementation downloadFile)
        {
            downloadFile.TotalBytesWritten = cursor.GetInt (cursor.GetColumnIndex (Android.App.DownloadManager.ColumnBytesDownloadedSoFar));
            downloadFile.TotalBytesExpected = cursor.GetInt (cursor.GetColumnIndex (Android.App.DownloadManager.ColumnTotalSizeBytes));

            switch (cursor.GetInt (cursor.GetColumnIndex (Android.App.DownloadManager.ColumnStatus))) {
            // Successful
            case 8:
                downloadFile.Status = DownloadFileStatus.COMPLETED;
                Queue.Remove (downloadFile);
                break;

            // Failed
            case 16:
                downloadFile.Status = DownloadFileStatus.FAILED;
                Queue.Remove (downloadFile);
                break;

            // Paused
            case 4:
                downloadFile.Status = DownloadFileStatus.PAUSED;
                break;

            // Pending
            case 1:
                downloadFile.Status = DownloadFileStatus.PENDING;
                break;

            // Running
            case 2:
                downloadFile.Status = DownloadFileStatus.RUNNING;
                break;
            }
        }
    }
}
