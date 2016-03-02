using System.Linq;
using Android.App;
using Android.Content;

namespace CrossDownloadManager.Android
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
            var downloadQueue = _downloadManager.Queue.Cast<AndroidDownloadFile> ().ToList ();

            long reference = intent.GetLongExtra (DownloadManager.ExtraDownloadId, -1);

            var downloadFile = downloadQueue.FirstOrDefault (f => f.Id == reference);
            if (downloadFile != null) {
                var query = new DownloadManager.Query ();
                query.SetFilterById (reference);

                var cursor = ((DownloadManager)context.GetSystemService (Context.DownloadService)).InvokeQuery (query);
                cursor.MoveToFirst ();

                int bytesDownloaded = cursor.GetInt (cursor.GetColumnIndex (DownloadManager.ColumnBytesDownloadedSoFar));
                int status = cursor.GetInt (cursor.GetColumnIndex (DownloadManager.ColumnStatus));
                int totalSizeBytes = cursor.GetInt (cursor.GetColumnIndex (DownloadManager.ColumnTotalSizeBytes));

                downloadFile.TotalBytesWritten = bytesDownloaded;
                downloadFile.TotalBytesExpected = totalSizeBytes;

                switch (status) {
                // Successful
                case 8:
                    downloadFile.Status = DownloadStatus.COMPLETED;
                    _downloadManager.Queue.Remove (downloadFile);
                    break;

                // Failed
                case 16:
                    downloadFile.Status = DownloadStatus.FAILED;
                    _downloadManager.Queue.Remove (downloadFile);
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
}