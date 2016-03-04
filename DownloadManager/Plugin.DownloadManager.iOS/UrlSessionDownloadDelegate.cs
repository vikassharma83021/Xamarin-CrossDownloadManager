using System.Linq;
using Foundation;
using Plugin.DownloadManager.Abstractions;

namespace Plugin.DownloadManager
{
    public class UrlSessionDownloadDelegate : NSUrlSessionDownloadDelegate
    {
        public DownloadManagerImplementation Controller;

        DownloadFileImplementation getDownloadFileByTask (NSUrlSessionDownloadTask downloadTask)
        {
            return Controller.Queue
                .Cast<DownloadFileImplementation> ()
                .FirstOrDefault (
                    i => i.Task != null &&
                    (int)i.Task.TaskIdentifier == (int)downloadTask.TaskIdentifier
                );
        }

        /**
         * A Task was resumed (or started ..)
         */
        public override void DidResume (NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long resumeFileOffset, long expectedTotalBytes)
        {
            var file = getDownloadFileByTask (downloadTask);
            if (file == null) {
                downloadTask.Cancel ();
                return;
            }

            file.Status = DownloadFileStatus.RUNNING;
        }

        /**
         * The Task keeps receiving data. Keep track of the current progress ...
         */
        public override void DidWriteData (NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long bytesWritten, long totalBytesWritten, long totalBytesExpectedToWrite)
        {
            var file = getDownloadFileByTask (downloadTask);
            if (file == null) {
                downloadTask.Cancel ();
                return;
            }

            file.Status = DownloadFileStatus.RUNNING;

            file.TotalBytesExpected = totalBytesExpectedToWrite;
            file.TotalBytesWritten = totalBytesWritten;
        }

        public override void DidFinishDownloading (NSUrlSession session, NSUrlSessionDownloadTask downloadTask, NSUrl location)
        {
            var file = getDownloadFileByTask (downloadTask);
            if (file == null) {
                downloadTask.Cancel ();
                return;
            }

            MoveDownloadedFile (file, location);
        }

        /**
         * Move the downloaded file to it's destination and remove it from the download-queue.
         */
        public void MoveDownloadedFile (DownloadFileImplementation file, NSUrl location)
        {
            NSFileManager fileManager = NSFileManager.DefaultManager;

            var destinationURL = new NSUrl (Controller.UriForDownloadedFile (file));
            NSError removeCopy;
            NSError errorCopy;

            fileManager.Remove (destinationURL, out removeCopy);
            bool success = fileManager.Copy (location, destinationURL, out errorCopy);

            if (success) {
                file.Status = DownloadFileStatus.COMPLETED;
            } else {
                file.Status = DownloadFileStatus.CANCELED;
            }

            Controller.Queue.Remove (file);
        }

        public override void DidFinishEventsForBackgroundSession(NSUrlSession session)
        {
            var handler = CrossDownloadManager.BackgroundSessionCompletionHandler;
            if (handler != null)
            {
                CrossDownloadManager.BackgroundSessionCompletionHandler = null;
                handler();
            }
        }
    }
}
