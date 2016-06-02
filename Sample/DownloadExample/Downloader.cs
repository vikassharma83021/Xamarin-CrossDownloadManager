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
            File = CrossDownloadManager.Current.CreateDownloadFile (
                "http://www.speedtestx.de/testfiles/data_10mb.test"
                // If you need, you can add a dictionary of headers you need.
                //, new Dictionary<string, string> {
                //    { "Cookie", "LetMeDownload=1;" },
                //    { "Authorization", "Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==" }
                //}
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

