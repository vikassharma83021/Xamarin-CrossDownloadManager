using Windows.Networking.BackgroundTransfer;
using Plugin.DownloadManager.Abstractions;

namespace Plugin.DownloadManager
{
    public static class Helper
    {
        public static DownloadFileStatus ToDownloadFileStatus(this BackgroundTransferStatus status)
        {
            switch (status)
            {
                case BackgroundTransferStatus.Running:
                    return DownloadFileStatus.RUNNING;

                case BackgroundTransferStatus.PausedByApplication:
                    return DownloadFileStatus.PAUSED;

                case BackgroundTransferStatus.PausedCostedNetwork:
                    return DownloadFileStatus.PAUSED;

                case BackgroundTransferStatus.PausedNoNetwork:
                    return DownloadFileStatus.PAUSED;

                case BackgroundTransferStatus.Error:
                    return DownloadFileStatus.FAILED;

                case BackgroundTransferStatus.Completed:
                    return DownloadFileStatus.COMPLETED;

                case BackgroundTransferStatus.Canceled:
                    return DownloadFileStatus.CANCELED;

                case BackgroundTransferStatus.Idle:
                case BackgroundTransferStatus.PausedSystemPolicy:
                    return DownloadFileStatus.PAUSED;
            }
            return DownloadFileStatus.INITIALIZED;
        }
    }
}
