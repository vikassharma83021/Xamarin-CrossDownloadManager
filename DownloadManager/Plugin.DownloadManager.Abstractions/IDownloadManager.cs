using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

namespace Plugin.DownloadManager.Abstractions
{
    public interface IDownloadManager
    {
        ObservableCollection<IDownloadFile> Queue { get; }

        Func<IDownloadFile, string> UriForDownloadedFile { get; set; }

        IDownloadFile CreateDownloadFile (string url, IDictionary<string, string> headers);

        void Start (IDownloadFile file);

        void Abort (IDownloadFile file);

        void AbortAll ();
    }
}
