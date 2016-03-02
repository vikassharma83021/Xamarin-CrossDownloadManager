using System.Collections.Generic;
using System.ComponentModel;

namespace CrossDownloadManager
{
    public enum DownloadStatus
    {
        PENDING,
        RUNNING,
        PAUSED,
        COMPLETED,
        CANCELED,
        FAILED
    }

    public interface ICrossDownloadFile : INotifyPropertyChanged
    {
        string Url { get; }

        IDictionary<string, string> Headers { get; }

        DownloadStatus Status { get; }

        float TotalBytesExpected { get; }

        float TotalBytesWritten { get; }
    }
}