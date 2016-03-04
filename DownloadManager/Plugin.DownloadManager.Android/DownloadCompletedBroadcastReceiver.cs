using System;
using System.Linq;
using Android.Content;
using Plugin.DownloadManager.Abstractions;

namespace Plugin.DownloadManager.Droid
{
    public class DownloadCompletedBroadcastReceiver : BroadcastReceiver
    {
        Func<IDownloadManager> _downloadManagerAction;

        public DownloadCompletedBroadcastReceiver (Func<IDownloadManager> downloadManagerAction)
        {
            _downloadManagerAction = downloadManagerAction;
        }

        public override void OnReceive (Context context, Intent intent)
        {
            long reference = intent.GetLongExtra (Android.App.DownloadManager.ExtraDownloadId, -1);

            var downloadFile = _downloadManagerAction().Queue.Cast<DownloadFileImplementation> ().FirstOrDefault (f => f.Id == reference);
            if (downloadFile != null) {
                var query = new Android.App.DownloadManager.Query ();
                query.SetFilterById (downloadFile.Id);

                try {
                    using (var cursor = ((Android.App.DownloadManager)context.GetSystemService (Context.DownloadService)).InvokeQuery (query)) {
                        while (cursor.MoveToNext ()) {
                            ((DownloadManagerImplementation)_downloadManagerAction()).UpdateFileProperties (cursor, downloadFile);
                        }
                    }
                } catch (Android.Database.Sqlite.SQLiteException) {
                    // I lately got an exception that the database was unaccessible ...
                }
            }
        }
    }
}
