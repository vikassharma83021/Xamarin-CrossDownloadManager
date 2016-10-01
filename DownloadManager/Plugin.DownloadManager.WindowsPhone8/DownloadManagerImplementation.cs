using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Plugin.DownloadManager.Abstractions;

namespace Plugin.DownloadManager
{
    /// <summary>
    /// The WindowsPhone8 implementation of the download manager.
    /// </summary>
    public class DownloadManagerImplementation : IDownloadManager
    {
        public ObservableCollection<IDownloadFile> Queue { get; private set; }

        public Func<IDownloadFile, string> PathNameForDownloadedFile { get; set; }

        public bool MobileNetworkAllowed
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public IDownloadFile CreateDownloadFile (string url)
        {
            throw new NotImplementedException ();
        }

        public IDownloadFile CreateDownloadFile (string url, IDictionary<string, string> headers)
        {
            throw new NotImplementedException ();
        }

        public void Start (IDownloadFile i)
        {
            throw new NotImplementedException ();
        }

        public void Abort (IDownloadFile i)
        {
            throw new NotImplementedException ();
        }

        public void AbortAll ()
        {
            throw new NotImplementedException ();
        }
    }
}
