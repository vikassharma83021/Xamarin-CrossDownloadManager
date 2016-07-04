using System.Linq;
using Foundation;
using Plugin.DownloadManager.Abstractions;

namespace Plugin.DownloadManager
{
    public class UrlSessionDownloadDelegate : NSUrlSessionDownloadDelegate
    {
        public DownloadManagerImplementation Controller;

        protected DownloadFileImplementation getDownloadFileByTask (NSUrlSessionTask downloadTask)
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

        public override void DidCompleteWithError (Foundation.NSUrlSession session, Foundation.NSUrlSessionTask task, Foundation.NSError error)
        {
            var file = getDownloadFileByTask (task);
            if (file == null)
                return;

            file.Status = DownloadFileStatus.FAILED;
            file.StatusDetails = error.LocalizedDescription;

            Controller.Queue.Remove (file);
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

            var success = true;
            if (Controller.PathNameForDownloadedFile != null) {
                var destinationPathName = Controller.PathNameForDownloadedFile (file);
                if (destinationPathName != null) {
                    success = MoveDownloadedFile (file, location, destinationPathName);
                }
            }

            // If the file destination is unknown or was moved successfully ...
            if (success) {
                file.Status = DownloadFileStatus.COMPLETED;
            }

            Controller.Queue.Remove (file);
        }

        /**
         * Move the downloaded file to it's destination
         */
        public bool MoveDownloadedFile (DownloadFileImplementation file, NSUrl location, string destinationPathName)
        {
            NSFileManager fileManager = NSFileManager.DefaultManager;

            var destinationURL = new NSUrl (destinationPathName, false);
            NSError removeCopy;
            NSError errorCopy;

            fileManager.Remove (destinationURL, out removeCopy);
            var success = fileManager.Copy (location, destinationURL, out errorCopy);

            if (!success) {
                file.StatusDetails = errorCopy.LocalizedDescription;
                file.Status = DownloadFileStatus.FAILED;
            }

            return success;
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
