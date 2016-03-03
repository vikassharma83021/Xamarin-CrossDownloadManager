using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.DownloadManager.Abstractions
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

    public interface IDownloadFile : INotifyPropertyChanged
    {
        string Url { get; }

        IDictionary<string, string> Headers { get; }

        DownloadStatus Status { get; }

        float TotalBytesExpected { get; }

        float TotalBytesWritten { get; }
    }
}
