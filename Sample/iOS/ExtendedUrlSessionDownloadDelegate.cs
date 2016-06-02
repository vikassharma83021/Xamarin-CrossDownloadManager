using Plugin.DownloadManager;

namespace DownloadExample.iOS
{
    // This class let you do interact with the downloading tasks.
    // See https://developer.apple.com/library/ios/documentation/Foundation/Reference/NSURLSessionDownloadDelegate_protocol/index.html
    public class ExtendedUrlSessionDownloadDelegate : UrlSessionDownloadDelegate
    {
        public override void DidCompleteWithError (Foundation.NSUrlSession session, Foundation.NSUrlSessionTask task, Foundation.NSError error)
        {
            // Add a breakpoint here if you encounter any errors.
        }

        public override void DidFinishEventsForBackgroundSession (Foundation.NSUrlSession session)
        {
            // If you want to notify the users, that all files are downloaded, do it here - before the base-method is called.

            base.DidFinishEventsForBackgroundSession (session);
        }
    }
}

