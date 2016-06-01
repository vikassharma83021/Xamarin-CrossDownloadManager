using System.Collections.Generic;
using Plugin.DownloadManager;
using Plugin.DownloadManager.Abstractions;

namespace DownloadExample
{
    public class Downloader
    {
        public IDownloadFile File;

        public void StartDownloading ()
        {
            File = CrossDownloadManager.Current.CreateDownloadFile(
                "http://www.speedtestx.de/testfiles/data_10mb.test",
                new Dictionary<string, string>()
            );

            CrossDownloadManager.Current.Start (File);
        }

        public void AbortDownloading ()
        {
            CrossDownloadManager.Current.Abort (File);
        }

        public bool IsDownloading ()
        {
            if (File != null) {
                switch (File.Status) {
                case DownloadFileStatus.PAUSED:
                case DownloadFileStatus.PENDING:
                case DownloadFileStatus.RUNNING:
                    return true;
                }
            }

            return false;
        }
   }
}

