using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Content;

namespace CrossDownloadManager.Android
{
    public class AndroidDownloadManager : ICrossDownloadManager
    {
        DownloadManager _downloadManager;

        public ObservableCollection<ICrossDownloadFile> Queue { get; private set; }

        public Func<ICrossDownloadFile, string> UriForDownloadedFile { get; set; }

        public AndroidDownloadManager (Context applicationContext)
        {
            Queue = new ObservableCollection<ICrossDownloadFile> ();

            _downloadManager = (DownloadManager)applicationContext.GetSystemService (Context.DownloadService);
        }

        public ICrossDownloadFile CreateDownloadFile (string url, IDictionary<string, string> headers)
        {
            return new AndroidDownloadFile (url, headers);
        }

        public void Start (ICrossDownloadFile i)
        {
            var file = (AndroidDownloadFile)i;

            file.StartDownload (_downloadManager, UriForDownloadedFile (file));
            Queue.Add (file);
        }

        public void Abort (ICrossDownloadFile i)
        {
            var file = (AndroidDownloadFile)i;

            file.Status = DownloadStatus.CANCELED;
            _downloadManager.Remove (file.Id);
            Queue.Remove (file);
        }

        public void AbortAll ()
        {
            foreach (var file in Queue) {
                Abort (file);
            }
        }
    }
}