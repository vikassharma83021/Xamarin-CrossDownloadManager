using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Foundation;
using Plugin.DownloadManager.Abstractions;
using UIKit;

namespace Plugin.DownloadManager
{
    /// <summary>
    /// The iOS implementation of the download manager.
    /// </summary>
    public class DownloadManagerImplementation : IDownloadManager
    {
        private string _identifier => NSBundle.MainBundle.BundleIdentifier + ".BackgroundTransferSession";

        NSUrlSession _session;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private IList<IDownloadFile> _queue;

        public IEnumerable<IDownloadFile> Queue {
            get {
                lock(_queue) {
                    return _queue.ToList();
                }
            }
        }

        public Func<IDownloadFile, string> PathNameForDownloadedFile { get; set; }

        public DownloadManagerImplementation (UrlSessionDownloadDelegate sessionDownloadDelegate)
        {
            _queue = new List<IDownloadFile> ();

            _session = InitBackgroundSession (sessionDownloadDelegate);

            // Reinitialize tasks that were started before the app was terminated or suspended
            _session.GetTasks2((NSUrlSessionTask[] dataTasks, NSUrlSessionTask[] uploadTasks, NSUrlSessionTask[] downloadTasks) => {
                foreach (var task in downloadTasks) {
                    AddFile(new DownloadFileImplementation(task));
                }
            });
        }

        public IDownloadFile CreateDownloadFile (string url)
        {
            return CreateDownloadFile (url, new Dictionary<string, string> ());
        }

        public IDownloadFile CreateDownloadFile (string url, IDictionary<string, string> headers)
        {
            return new DownloadFileImplementation (url, headers);
        }

        public void Start (IDownloadFile i, bool mobileNetworkAllowed = true)
        {
            var file = (DownloadFileImplementation)i;

            file.StartDownload (_session, mobileNetworkAllowed);
            AddFile(file);
        }

        public void Abort (IDownloadFile i)
        {
            var file = (DownloadFileImplementation)i;

            file.Status = DownloadFileStatus.CANCELED;
            if (file.Task != null) {
                file.Task.Cancel ();
            }

            RemoveFile(file);
        }

        public void AbortAll ()
        {
            foreach (var file in Queue) {
                Abort (file);
            }
        }

        /**
         * We initialize the background session with the following options
         * - nil as queue: The method, called on events could end up on any thread
         * - Only one connection per host
         */
        NSUrlSession InitBackgroundSession (UrlSessionDownloadDelegate sessionDownloadDelegate)
        {
            sessionDownloadDelegate.Controller = this;

            NSUrlSessionConfiguration configuration;

            if (UIDevice.CurrentDevice.CheckSystemVersion (8, 0)) {
                using (configuration = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration(_identifier)) {
                    return createSession(configuration, sessionDownloadDelegate);
                }
            } else {
                using(configuration = NSUrlSessionConfiguration.BackgroundSessionConfiguration(_identifier)) {
                    return createSession(configuration, sessionDownloadDelegate);
                };
            }
        }

        private NSUrlSession createSession(NSUrlSessionConfiguration configuration, UrlSessionDownloadDelegate sessionDownloadDelegate) {
            configuration.HttpMaximumConnectionsPerHost = 1;

            return NSUrlSession.FromConfiguration(configuration, sessionDownloadDelegate, null);
        }

        protected internal void AddFile(IDownloadFile file)
        {
            lock (_queue) {
                _queue.Add(file);
            }

            if (CollectionChanged != null) {
                CollectionChanged.Invoke(Queue, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, file, null));
            }
        }

        protected internal void RemoveFile(IDownloadFile file)
        {
            lock (_queue) {
                _queue.Remove(file);
            }

            if (CollectionChanged != null) {
                CollectionChanged.Invoke(Queue, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, null, file));
            }
        }
    }
}
