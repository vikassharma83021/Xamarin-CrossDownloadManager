using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Foundation;
using UIKit;

namespace CrossDownloadManager.Ios
{
    public class IosDownloadManager : ICrossDownloadManager
    {
        const string _identifier = "org.brunstad.bmm.BackgroundTransferSession";

        NSUrlSession _session;

        public ObservableCollection<ICrossDownloadFile> Queue { get; private set; }

        public Func<ICrossDownloadFile, string> UriForDownloadedFile { get; set; }

        public IosDownloadManager (UrlSessionDownloadDelegate sessionDownloadDelegate)
        {
            Queue = new ObservableCollection<ICrossDownloadFile> ();

            _session = InitBackgroundSession (sessionDownloadDelegate);

            // Reinitialize tasks that were started before the app was terminated or suspended
            _session.GetAllTasks ((NSUrlSessionTask [] tasks) => {
                foreach (var task in tasks) {
                    if (task is NSUrlSessionDownloadTask) {
                        Queue.Add (new IosDownloadFile (task));
                    }
                }
            });
        }

        public ICrossDownloadFile CreateDownloadFile (string url, IDictionary<string, string> headers)
        {
            return new IosDownloadFile (url, headers);
        }

        public void Start (ICrossDownloadFile i)
        {
            var file = (IosDownloadFile)i;
            file.StartDownload (_session);
            Queue.Add (file);
        }

        public void Abort (ICrossDownloadFile i)
        {
            var file = (IosDownloadFile)i;

            file.Status = DownloadStatus.CANCELED;
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