using System.Linq;
using Android.Content;

namespace Plugin.DownloadManager
{
    public class DownloadCompletedBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive (Context context, Intent intent)
        {
            long reference = intent.GetLongExtra (Android.App.DownloadManager.ExtraDownloadId, -1);

            var downloadFile = CrossDownloadManager.Current.Queue.Cast<DownloadFileImplementation> ().FirstOrDefault (f => f.Id == reference);
            if (downloadFile != null) {
                var query = new Android.App.DownloadManager.Query ();
                query.SetFilterById (downloadFile.Id);

                try {
                    using (var cursor = ((Android.App.DownloadManager)context.GetSystemService (Context.DownloadService)).InvokeQuery (query)) {
                        while (cursor.MoveToNext ()) {
                            ((DownloadManagerImplementation)CrossDownloadManager.Current).UpdateFileProperties (cursor, downloadFile);
                        }
                        cursor?.Close();
                    }
                } catch (Android.Database.Sqlite.SQLiteException) {
                    // I lately got an exception that the database was unaccessible ...
                }
            }
        }
    }
}
