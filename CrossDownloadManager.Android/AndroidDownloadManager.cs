using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Database;
using Android.Content;

namespace CrossDownloadManager.Android
{
    public class AndroidDownloadManager : ICrossDownloadManager
    {
        Android.OS.Handler _downloadWatcherHandler;
        Java.Lang.Runnable _downloadWatcherHandlerRunnable;

        DownloadManager _downloadManager;

        public ObservableCollection<ICrossDownloadFile> Queue { get; private set; }

        public Func<ICrossDownloadFile, string> UriForDownloadedFile { get; set; }

        public AndroidDownloadManager (Context applicationContext)
        {
            Queue = new ObservableCollection<ICrossDownloadFile> ();

            _downloadManager = (DownloadManager)applicationContext.GetSystemService (Context.DownloadService);

            // Check sequentially if parameters for any of the registered downloads changed
            StartDownloadWatcher ();
        }

        public ICrossDownloadFile CreateDownloadFile (string url, IDictionary<string, string> headers)
        {
            return new AndroidDownloadFile (url, headers);
        }

        public void Start (ICrossDownloadFile i)
        {
            var file = (AndroidDownloadFile)i;

            file.StartDownload (_downloadManager, UriForDownloadedFile (file));
            Queue.Add (file);
        }

        public void Abort (ICrossDownloadFile i)
        {
            var file = (AndroidDownloadFile)i;

            file.Status = DownloadStatus.CANCELED;
            _downloadManager.Remove (file.Id);
            Queue.Remove (file);
        }

        public void AbortAll ()
        {
            foreach (var file in Queue.ToList()) {
                Abort (file);
            }
        }

        void LoopOnDownloads (Action<ICursor> runnable)
        {
            // Reinitialize downloads that were started before the app was terminated or suspended
            var query = new DownloadManager.Query ();
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

        void StartDownloadWatcher ()
        {
            // Create an instance for a runnable-handler
            _downloadWatcherHandler = new Android.OS.Handler ();

            // Create a runnable, restarting itself to update every file in the queue
            _downloadWatcherHandlerRunnable = new Java.Lang.Runnable (() => {

                // Loop throught all files in the system-queue and update the data in the local queue
                LoopOnDownloads(cursor => UpdateFileProperties (cursor));

                var nextExecution = (Queue.Count == 0) ? 1000 : 100;
                _downloadWatcherHandler.PostDelayed (_downloadWatcherHandlerRunnable, nextExecution);
            });

            // Start this playing handler immediately
            _downloadWatcherHandler.PostDelayed (_downloadWatcherHandlerRunnable, 0);
        }

        public void UpdateFileProperties (ICursor cursor)
        {
            int id = cursor.GetInt (cursor.GetColumnIndex (DownloadManager.ColumnId));
            var downloadFile = Queue.Cast<AndroidDownloadFile> ().FirstOrDefault (f => f.Id == id);

            if (downloadFile != null) {
                UpdateFileProperties (cursor, downloadFile);
            }
        }

        /**
         * Update the properties for a file by it's cursor.
         * This method should be called in an interval and on reinitialization.
         */
        public void UpdateFileProperties (ICursor cursor, AndroidDownloadFile downloadFile)
        {
            downloadFile.TotalBytesWritten = cursor.GetInt (cursor.GetColumnIndex (DownloadManager.ColumnBytesDownloadedSoFar));
            downloadFile.TotalBytesExpected = cursor.GetInt (cursor.GetColumnIndex (DownloadManager.ColumnTotalSizeBytes));

            switch (cursor.GetInt (cursor.GetColumnIndex (DownloadManager.ColumnStatus))) {
                // Successful
                case 8:
                downloadFile.Status = DownloadStatus.COMPLETED;
                Queue.Remove (downloadFile);
                break;

                // Failed
                case 16:
                downloadFile.Status = DownloadStatus.FAILED;
                Queue.Remove (downloadFile);
                break;

                // Paused
                case 4:
                downloadFile.Status = DownloadStatus.PAUSED;
                break;

                // Pending
                case 1:
                downloadFile.Status = DownloadStatus.PENDING;
                break;

                // Running
                case 2:
                downloadFile.Status = DownloadStatus.RUNNING;
                break;
            }
        }
    }
}