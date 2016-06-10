using System.Linq;
using Foundation;
using Plugin.DownloadManager.Abstractions;

namespace Plugin.DownloadManager
{
    public class UrlSessionDownloadDelegate : NSUrlSessionDownloadDelegate
    {
        public DownloadManagerImplementation Controller;

        protected DownloadFileImplementation getDownloadFileByTask (NSUrlSessionDownloadTask downloadTask)
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
            var file = getDownloadFileByTask ((NSUrlSessionDownloadTask)task);
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

            MoveDownloadedFile (file, location);
        }

        /**
         * Move the downloaded file to it's destination and remove it from the download-queue.
         */
        public void MoveDownloadedFile (DownloadFileImplementation file, NSUrl location)
        {
            NSFileManager fileManager = NSFileManager.DefaultManager;

            var destinationURL = new NSUrl (Controller.PathNameForDownloadedFile (file), false);
            NSError removeCopy;
            NSError errorCopy;

            fileManager.Remove (destinationURL, out removeCopy);
            bool success = fileManager.Copy (location, destinationURL, out errorCopy);

            if (success) {
                file.Status = DownloadFileStatus.COMPLETED;
            } else {
                file.StatusDetails = errorCopy.LocalizedDescription;
                file.Status = DownloadFileStatus.FAILED;
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


        public override void DidReceiveChallenge (NSUrlSession session, NSUrlSessionTask task, NSUrlAuthenticationChallenge challenge, System.Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential> completionHandler) { }

        public override void DidSendBodyData (NSUrlSession session, NSUrlSessionTask task, long bytesSent, long totalBytesSent, long totalBytesExpectedToSend) { }

        public override void NeedNewBodyStream (NSUrlSession session, NSUrlSessionTask task, System.Action<NSInputStream> completionHandler) { }

        public override void WillPerformHttpRedirection (NSUrlSession session, NSUrlSessionTask task, NSHttpUrlResponse response, NSUrlRequest newRequest, System.Action<NSUrlRequest> completionHandler) { }

        public override void DidBecomeInvalid (NSUrlSession session, NSError error) { }

        public override void DidReceiveChallenge (NSUrlSession session, NSUrlAuthenticationChallenge challenge, System.Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential> completionHandler) { }
    }
}
