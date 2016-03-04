using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Plugin.DownloadManager.Abstractions
{
    public interface IDownloadManager
    {
        ObservableCollection<IDownloadFile> Queue { get; }

        IDownloadFile CreateDownloadFile (string url, IDictionary<string, string> headers);

        void Start (IDownloadFile file);

        void Abort (IDownloadFile file);

        void AbortAll ();
    }
}
