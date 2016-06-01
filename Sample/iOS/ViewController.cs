using System;
using Plugin.DownloadManager;
using Plugin.DownloadManager.Abstractions;
using UIKit;

namespace DownloadExample.iOS
{
    public partial class ViewController : UIViewController
    {
        int count = 1;

        public ViewController (IntPtr handle) : base (handle)
        {
            // If you want to set some extensional things - like an error-handler to see why a download failed ...
            //CrossDownloadManager.UrlSessionDownloadDelegate = new ExtendedUrlSessionDownloadDelegate ();
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            var foo = new Downloader ();

            // Perform any additional setup after loading the view, typically from a nib.
            Button.AccessibilityIdentifier = "myButton";
            Button.TouchUpInside += delegate {
                // If already downloading, abort it.
                if (foo.IsDownloading ()) {
                    foo.AbortDownloading ();
                    Button.SetTitle ("Download aborted.", UIControlState.Normal);
                    return;
                }

                Button.SetTitle ("Start downloading ...", UIControlState.Normal);

                foo.StartDownloading ();

                foo.File.PropertyChanged += (sender, e) => {
                    // Update UI if download-status changed.
                    if (e.PropertyName == "Status") {
                        switch (((IDownloadFile)sender).Status) {
                        case DownloadFileStatus.COMPLETED:
                        case DownloadFileStatus.FAILED:
                        case DownloadFileStatus.CANCELED:
                            InvokeOnMainThread (() => {
                                Button.SetTitle ("Downloading finished.", UIControlState.Normal);
                            });
                            break;
                        }
                    }

                    // Update UI while donwloading.
                    if (e.PropertyName == "TotalBytesWritten" || e.PropertyName == "TotalBytesExpected") {
                        var bytesExpected = ((IDownloadFile)sender).TotalBytesExpected;
                        var bytesWritten = ((IDownloadFile)sender).TotalBytesWritten;

                        if (bytesExpected > 0) {
                            var percentage = Math.Round (bytesWritten / bytesExpected * 100);
                            InvokeOnMainThread (() => {
                                Button.SetTitle ("Downloading (" + percentage + "%)", UIControlState.Normal);
                            }); 
                        }
                    }
                };
            };
        }
    }
}
