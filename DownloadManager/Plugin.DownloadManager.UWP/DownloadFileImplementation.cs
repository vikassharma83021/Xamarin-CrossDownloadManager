using Plugin.DownloadManager.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;

namespace Plugin.DownloadManager
{
    public class DownloadFileImplementation : IDownloadFile
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DownloadOperation DownloadOperation;

        private CancellationTokenSource _cancellationToken;

        public string Url { get; }

        public IDictionary<string, string> Headers { get; }

        DownloadFileStatus _status;

        public DownloadFileStatus Status {
            get {
                return _status;
            }
            set
            {
                if (Equals(_status, value)) return;
                _status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        string _statusDetails;

        public string StatusDetails
        {
            get {
                return _statusDetails;
            }
            set
            {
                if (Equals(_statusDetails, value)) return;
                _statusDetails = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusDetails)));
            }
        }

        private float _totalBytesExpected;

        public float TotalBytesExpected
        {
            get {
                return _totalBytesExpected;
            }
            set
            {
                if (Equals(_totalBytesExpected, value)) return;
                _totalBytesExpected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalBytesExpected)));
            }
        }

        private float _totalBytesWritten;

        public float TotalBytesWritten
        {
            get {
                return _totalBytesWritten;
            }
            set
            {
                if (Equals(_totalBytesWritten, value)) return;
                _totalBytesWritten = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalBytesWritten)));
            }
        }

        public DownloadFileImplementation(string url, IDictionary<string, string> headers)
        {
            Url = url;
            Headers = headers;

            Status = DownloadFileStatus.INITIALIZED;
        }

        public DownloadFileImplementation(DownloadOperation downloadOperation)
        {
            DownloadOperation = downloadOperation;

            var progress = new Progress<DownloadOperation>(ProgressChanged);
            _cancellationToken = new CancellationTokenSource();

            DownloadOperation.AttachAsync().AsTask(_cancellationToken.Token, progress);
        }

        internal void StartDownloadAsync(string destinationPathName, bool mobileNetworkAllowed)
        {
            var downloader = new BackgroundDownloader();

            var downloadUrl = new Uri(Url);

            if (Headers != null)
            {
                foreach (var header in Headers)
                {
                    downloader.SetRequestHeader(header.Key, header.Value);
                }
            }

            if (!mobileNetworkAllowed)
            {
                downloader.CostPolicy = BackgroundTransferCostPolicy.UnrestrictedOnly;
            }

            StorageFile file;
            if (destinationPathName != null)
            {
                var folder = StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(destinationPathName)).AsTask().Result;
                file = folder.CreateFileAsync(Path.GetFileName(destinationPathName), CreationCollisionOption.ReplaceExisting).AsTask().Result;
            }
            else
            {
                var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                file = folder.CreateFileAsync(downloadUrl.Segments.Last(), CreationCollisionOption.GenerateUniqueName).AsTask().Result;
            }

            DownloadOperation = downloader.CreateDownload(downloadUrl, file);

            var progress = new Progress<DownloadOperation>(ProgressChanged);
            _cancellationToken = new CancellationTokenSource();

            DownloadOperation.StartAsync().AsTask(_cancellationToken.Token, progress);
        }

        private void ProgressChanged(DownloadOperation downloadOperation)
        {
            TotalBytesExpected = downloadOperation.Progress.TotalBytesToReceive;
            TotalBytesWritten = downloadOperation.Progress.BytesReceived;

            switch (downloadOperation.Progress.Status)
            {
                case BackgroundTransferStatus.Running:
                    Status = DownloadFileStatus.RUNNING;
                    break;

                case BackgroundTransferStatus.PausedByApplication:
                    Status = DownloadFileStatus.PAUSED;
                    break;

                case BackgroundTransferStatus.PausedCostedNetwork:
                    Status = DownloadFileStatus.PAUSED;
                    break;

                case BackgroundTransferStatus.PausedNoNetwork:
                    Status = DownloadFileStatus.PAUSED;
                    break;

                case BackgroundTransferStatus.Error:
                    Status = DownloadFileStatus.FAILED;
                    break;

                case BackgroundTransferStatus.Completed:
                    Status = DownloadFileStatus.COMPLETED;
                    break;

                case BackgroundTransferStatus.Canceled:
                    Status = DownloadFileStatus.CANCELED;
                    break;

                case BackgroundTransferStatus.Idle:
                    break;

                case BackgroundTransferStatus.PausedSystemPolicy:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        internal void Cancel()
        {
            Status = DownloadFileStatus.CANCELED;
            if (_cancellationToken != null)
            {
                _cancellationToken.Cancel();
                _cancellationToken = null;
            }
        }
    }
}
