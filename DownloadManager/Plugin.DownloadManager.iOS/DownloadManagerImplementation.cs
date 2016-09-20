using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        const string _identifier = "org.brunstad.bmm.BackgroundTransferSession";

        NSUrlSession _session;

        public ObservableCollection<IDownloadFile> Queue { get; private set; }

        public Func<IDownloadFile, string> PathNameForDownloadedFile { get; set; }

		private UrlSessionDownloadDelegate _delegate;

		private bool _mobileNetworkAllowed = CrossDownloadManager.MobileNetworkAllowedByDefault;

		public bool MobileNetworkAllowed
		{
			get
			{
				return _mobileNetworkAllowed;
			}

			set
			{
				if (value != _mobileNetworkAllowed) {
					_mobileNetworkAllowed = value;

					//Reinitialize the BackgroundSession to apply the new configuration value
					_session = InitBackgroundSession (_delegate);

					if (Queue.Count > 0) {
						var downloadFiles = Queue.Cast<DownloadFileImplementation> ().ToList ();

						AbortAll ();

						downloadFiles.ForEach ((downloadFile) => {
							Start (downloadFile);
						});
					}
				}
			}
		}

		public DownloadManagerImplementation (UrlSessionDownloadDelegate sessionDownloadDelegate)
        {
            Queue = new ObservableCollection<IDownloadFile> ();

			_delegate = sessionDownloadDelegate;

			_session = InitBackgroundSession (_delegate);

            // Reinitialize tasks that were started before the app was terminated or suspended
            _session.GetTasks2((NSUrlSessionTask[] dataTasks, NSUrlSessionTask[] uploadTasks, NSUrlSessionTask[] downloadTasks) => {
                foreach (var task in downloadTasks) {
                    Queue.Add(new DownloadFileImplementation((NSUrlSessionDownloadTask)task));
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
            configuration.AllowsCellularAccess = _mobileNetworkAllowed;

            return NSUrlSession.FromConfiguration(configuration, sessionDownloadDelegate, null);
        }
    }
}
