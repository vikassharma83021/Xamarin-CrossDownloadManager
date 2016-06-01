using System;
using Plugin.DownloadManager;

namespace DownloadExample.iOS
{
    public class ExtendedUrlSessionDownloadDelegate : UrlSessionDownloadDelegate
    {
        public override void DidCompleteWithError (Foundation.NSUrlSession session, Foundation.NSUrlSessionTask task, Foundation.NSError error)
        {
            // Add a breakpoint here if you encounter any errors.
        }
    }
}

