using System.Linq;
using Android.App;
using Android.Content;

namespace Plugin.DownloadManager.Droid
{
    public class DownloadCompletedBroadcastReceiver : BroadcastReceiver
    {
        ICrossDownloadManager _downloadManager;

        public DownloadCompletedBroadcastReceiver (ICrossDownloadManager downloadManager)
        {
            _downloadManager = downloadManager;
        }

        public override void OnReceive (Context context, Intent intent)
        {
            long reference = intent.GetLongExtra (DownloadManager.ExtraDownloadId, -1);

            var downloadFile = _downloadManager.Queue.Cast<AndroidDownloadFile> ().FirstOrDefault (f => f.Id == reference);
            if (downloadFile != null) {
                var query = new DownloadManager.Query ();
                query.SetFilterById (downloadFile.Id);

                try {
                    using (var cursor = ((DownloadManager)context.GetSystemService (Context.DownloadService)).InvokeQuery (query)) {
                        while (cursor.MoveToNext ()) {
                            ((AndroidDownloadManager)_downloadManager).UpdateFileProperties (cursor, downloadFile);
                        }
                    }
                } catch (Android.Database.Sqlite.SQLiteException) {
                    // I lately got an exception that the database was unaccessible ...
                }
            }
        }
    }
}
