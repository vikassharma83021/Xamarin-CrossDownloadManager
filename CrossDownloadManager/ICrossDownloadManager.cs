using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CrossDownloadManager
{
    public interface ICrossDownloadManager
    {
        ObservableCollection<ICrossDownloadFile> Queue { get; }

        ICrossDownloadFile CreateDownloadFile (string url, IDictionary<string, string> headers);

        void Start (ICrossDownloadFile file);

        void Abort (ICrossDownloadFile file);

        void AbortAll ();
    }
}