using System.Collections.Generic;
using System.ComponentModel;
using Android.Database;
using Android.Net;
using Plugin.DownloadManager.Abstractions;

namespace Plugin.DownloadManager
{
    public class DownloadFileImplementation : IDownloadFile
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public long Id;

        public string Url { get; set; }

        public IDictionary<string, string> Headers { get; private set; }

        DownloadFileStatus _status;

        public DownloadFileStatus Status {
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

        string _statusDetails;

        public string StatusDetails {
            get {
                return _statusDetails;
            }
            set {
                if (!Equals (_statusDetails, value)) {
                    _statusDetails = value;
                    PropertyChanged?.Invoke (this, new PropertyChangedEventArgs ("StatusDetails"));
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
        public DownloadFileImplementation (string url, IDictionary<string, string> headers)
        {
            Url = url;
            Headers = headers;

            Status = DownloadFileStatus.PENDING;
        }

        /**
         * Reinitializing an object after the app restarted
         */
        public DownloadFileImplementation (ICursor cursor)
        {
            Id = cursor.GetLong (cursor.GetColumnIndex (Android.App.DownloadManager.ColumnBytesDownloadedSoFar));
            Url = cursor.GetString (cursor.GetColumnIndex (Android.App.DownloadManager.ColumnUri));
        }

        public void StartDownload (Android.App.DownloadManager downloadManager, string destinationPathName)
        {
            using (var downloadUrl = Uri.Parse (Url))
            using (var request = new Android.App.DownloadManager.Request (downloadUrl)) {

                foreach (var header in Headers) {
                    request.AddRequestHeader (header.Key, header.Value);
                }

                request.SetDestinationUri (Uri.FromFile (new Java.IO.File (destinationPathName)));

                Id = downloadManager.Enqueue (request);

                Status = DownloadFileStatus.RUNNING;
            }
        }
    }
}
