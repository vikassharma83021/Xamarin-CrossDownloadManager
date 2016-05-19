using System.Collections.Generic;
using System.ComponentModel;

namespace Plugin.DownloadManager.Abstractions
{
    /// <summary>
    /// The status of the download file.
    /// </summary>
    public enum DownloadFileStatus
    {
        /// <summary>
        /// The download is pending.
        /// </summary>
        PENDING,

        /// <summary>
        /// The download is still running.
        /// </summary>
        RUNNING,

        /// <summary>
        /// The download was paused.
        /// </summary>
        PAUSED,

        /// <summary>
        /// The download has completed.
        /// </summary>
        COMPLETED,

        /// <summary>
        /// The download was canceled.
        /// </summary>
        CANCELED,

        /// <summary>
        /// The download has failed. You'll find detailed information in the property StatusDetails.
        /// </summary>
        FAILED
    }

    /// <summary>
    /// Download file.
    /// </summary>
    public interface IDownloadFile : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the URL of the file to download.
        /// </summary>
        /// <value>The URL.</value>
        string Url { get; }

        /// <summary>
        /// The headers that are send along when requesting the URL.
        /// </summary>
        /// <value>The headers.</value>
        IDictionary<string, string> Headers { get; }

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>The status.</value>
        DownloadFileStatus Status { get; }

        /// <summary>
        /// Gets the status details. F.e. to get the reason why the download failed.
        /// </summary>
        /// <value>The status details.</value>
        string StatusDetails { get; }

        /// <summary>
        /// Gets the amount of bytes expected.
        /// </summary>
        /// <value>The total bytes expected.</value>
        float TotalBytesExpected { get; }

        /// <summary>
        /// Gets the amount of bytes written.
        /// </summary>
        /// <value>The total bytes written.</value>
        float TotalBytesWritten { get; }
    }
}
