using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Foundation;
using Plugin.DownloadManager.Abstractions;
using UIKit;

namespace Plugin.DownloadManager.iOS
{
    public class DownloadManagerImplementation : IDownloadManager
    {
        const string _identifier = "org.brunstad.bmm.BackgroundTransferSession";

        NSUrlSession _session;

        public ObservableCollection<IDownloadFile> Queue { get; private set; }

        public Func<IDownloadFile, string> UriForDownloadedFile { get; set; }

        public DownloadManagerImplementation (UrlSessionDownloadDelegate sessionDownloadDelegate)
        {
            Queue = new ObservableCollection<IDownloadFile> ();

            _session = InitBackgroundSession (sessionDownloadDelegate);

            // Reinitialize tasks that were started before the app was terminated or suspended
            _session.GetAllTasks ((NSUrlSessionTask [] tasks) => {
                foreach (var task in tasks) {
                    if (task is NSUrlSessionDownloadTask) {
                        Queue.Add (new DownloadFileImplementation ((NSUrlSessionDownloadTask)task));
                    }
                }
            });
        }

        public IDownloadFile CreateDownloadFile (string url, IDictionary<string, string> headers)
        {
            return new DownloadFileImplementation (url, headers);
        }

        public void Start (IDownloadFile i)
        {
            var file = (DownloadFileImplementation)i;

            file.StartDownload (_session);
            Queue.Add (file);
        }

        public void Abort (IDownloadFile i)
        {
            var file = (DownloadFileImplementation)i;

            file.Status = DownloadFileStatus.CANCELED;
            if (file.Task != null) {
                file.Task.Cancel ();
            }

            Queue.Remove (file);
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

            if (UIDevice.CurrentDevice.CheckSystemVersion (8, 0)) {
                using (var configuration = NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration (_identifier)) {
                    configuration.HttpMaximumConnectionsPerHost = 1;
                    return NSUrlSession.FromConfiguration (configuration, sessionDownloadDelegate, null);
                }
            } else {
                using (var configuration = NSUrlSessionConfiguration.BackgroundSessionConfiguration (_identifier)) {
                    configuration.HttpMaximumConnectionsPerHost = 1;
                    return NSUrlSession.FromConfiguration (configuration, sessionDownloadDelegate, null);
                }
            }
        }
    }
}
