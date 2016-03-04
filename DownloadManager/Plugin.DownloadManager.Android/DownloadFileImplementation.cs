using System.Collections.Generic;
using System.ComponentModel;
using Android.App;
using Android.Database;
using Android.Net;

namespace Plugin.DownloadManager.Droid
{
    public class AndroidDownloadFile : ICrossDownloadFile
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public long Id;

        public string Url { get; set; }

        public IDictionary<string, string> Headers { get; private set; }

        DownloadStatus _status;

        public DownloadStatus Status {
            get {
                return _status;
            }
            set {
                if (!Equals (_status, value)) {
                    _status = value;
                    PropertyChanged?.Invoke (this, new PropertyChangedEventArgs ("Status"));
                }
            }
        }

        float _totalBytesExpected = 0.0f;

        public float TotalBytesExpected {
            get {
                return _totalBytesExpected;
            }
            set {
                if (!Equals (_totalBytesExpected, value)) {
                    _totalBytesExpected = value;
                    PropertyChanged?.Invoke (this, new PropertyChangedEventArgs ("TotalBytesExpected"));
                }
            }
        }

        float _totalBytesWritten = 0.0f;

        public float TotalBytesWritten {
            get {
                return _totalBytesWritten;
            }
            set {
                if (!Equals (_totalBytesWritten, value)) {
                    _totalBytesWritten = value;
                    PropertyChanged?.Invoke (this, new PropertyChangedEventArgs ("TotalBytesWritten"));
                }
            }
        }

        /**
         * Initializing a new object to add it to the download-queue
         */
        public AndroidDownloadFile (string url, IDictionary<string, string> headers)
        {
            Url = url;
            Headers = headers;

            Status = DownloadStatus.PENDING;
        }

        /**
         * Reinitializing an object after the app restarted
         */
        public AndroidDownloadFile (ICursor cursor)
        {
            Id = cursor.GetLong (cursor.GetColumnIndex (DownloadManager.ColumnBytesDownloadedSoFar));
            Url = cursor.GetString (cursor.GetColumnIndex (DownloadManager.ColumnUri));
        }

        public void StartDownload (DownloadManager downloadManager, string destinationUri)
        {
            using (var downloadUrl = Uri.Parse (Url))
            using (var request = new DownloadManager.Request (downloadUrl)) {

                foreach (var header in Headers) {
                    request.AddRequestHeader (header.Key, header.Value);
                }

                request.SetDestinationUri (Uri.FromFile (new Java.IO.File (destinationUri)));

                Id = downloadManager.Enqueue (request);

                Status = DownloadStatus.RUNNING;
            }
        }
    }
}
