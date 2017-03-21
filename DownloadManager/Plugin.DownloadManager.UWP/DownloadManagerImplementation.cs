using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Plugin.DownloadManager.Abstractions;
using Windows.Networking.BackgroundTransfer;
using System.Linq;

namespace Plugin.DownloadManager
{
    /// <summary>
    /// The UWP implementation of the download manager.
    /// </summary>
    public class DownloadManagerImplementation : IDownloadManager
    {
        private readonly IList<IDownloadFile> _queue;

        public IEnumerable<IDownloadFile> Queue
        {
            get
            {
                lock (_queue)
                {
                    return _queue.ToList();
                }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public Func<IDownloadFile, string> PathNameForDownloadedFile { get; set; }

        public DownloadManagerImplementation()
        {
            _queue = new List<IDownloadFile>();

            // Enumerate outstanding downloads.
            BackgroundDownloader.GetCurrentDownloadsAsync().AsTask().ContinueWith((downloadOperationsTask) => {
                foreach (var downloadOperation in downloadOperationsTask.Result)
                {
                    var downloadFile = new DownloadFileImplementation(downloadOperation);
                    _queue.Add(downloadFile);
                }
            });
        }

        public IDownloadFile CreateDownloadFile(string url)
        {
            return CreateDownloadFile(url, new Dictionary<string, string>());
        }

        public IDownloadFile CreateDownloadFile(string url, IDictionary<string, string> headers)
        {
            return new DownloadFileImplementation(url, headers);
        }

        public void Start(IDownloadFile i, bool mobileNetworkAllowed = true)
        {
            var file = (DownloadFileImplementation)i;

            string destinationPathName = null;
            if (PathNameForDownloadedFile != null)
            {
                destinationPathName = PathNameForDownloadedFile(file);
            }
            
            file.StartDownloadAsync(destinationPathName, mobileNetworkAllowed);
        }

        public void Abort(IDownloadFile i)
        {
            var file = (DownloadFileImplementation)i;

            file.Cancel();

            RemoveFile(file);
        }

        public void AbortAll()
        {
            foreach (var file in Queue)
            {
                Abort(file);
            }
        }

        protected internal void AddFile(IDownloadFile file)
        {
            lock (_queue)
            {
                _queue.Add(file);
            }

            CollectionChanged?.Invoke(Queue, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, file));
        }

        protected internal void RemoveFile(IDownloadFile file)
        {
            lock (_queue)
            {
                _queue.Remove(file);
            }

            CollectionChanged?.Invoke(Queue, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, file));
        }
    }
}
