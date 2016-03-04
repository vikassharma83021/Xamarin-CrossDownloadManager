using System.Collections.Generic;
using System.ComponentModel;

namespace Plugin.DownloadManager.Abstractions
{
    public enum DownloadFileStatus
    {
        PENDING,
        RUNNING,
        PAUSED,
        COMPLETED,
        CANCELED,
        FAILED
    }

    public interface IDownloadFile : INotifyPropertyChanged
    {
        string Url { get; }

        IDictionary<string, string> Headers { get; }

        DownloadFileStatus Status { get; }

        float TotalBytesExpected { get; }

        float TotalBytesWritten { get; }
    }
}
