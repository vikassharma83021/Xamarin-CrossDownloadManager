using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Plugin.DownloadManager;
using Plugin.DownloadManager.Abstractions;
using System;
using System.Linq;
using System.IO;

namespace DownloadExample.Droid
{
    [Activity (Label = "Download Example", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        void InitDownloadManager ()
        {
            // Define where the files should be stored. MUST be an external storage. (see https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/10)
            // If you skip this, you neither need the permission `WRITE_EXTERNAL_STORAGE`.
            //CrossDownloadManager.Current.PathNameForDownloadedFile = new Func<IDownloadFile, string> (file => {
            //    string fileName = Android.Net.Uri.Parse (file.Url).Path.Split ('/').Last ();
            //    return Path.Combine (ApplicationContext.GetExternalFilesDir (Android.OS.Environment.DirectoryDownloads).AbsolutePath, fileName);
            //});
        }

        NotificationClickedBroadcastReceiver _receiverNotificationClicked;

        protected override void OnResume ()
        {
            base.OnResume ();

            _receiverNotificationClicked = new NotificationClickedBroadcastReceiver ();
            RegisterReceiver (
                _receiverNotificationClicked,
                new IntentFilter (DownloadManager.ActionNotificationClicked)
            );
        }

        protected override void OnPause ()
        {
            base.OnPause ();

            UnregisterReceiver (_receiverNotificationClicked);
        }

        protected override void OnCreate (Bundle savedInstanceState)
        {
            base.OnCreate (savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            InitDownloadManager ();

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button> (Resource.Id.myButton);

            var foo = new Downloader ();

            button.Click += delegate {
                // If already downloading, abort it.
                if (foo.IsDownloading()) {
                    foo.AbortDownloading ();
                    button.Text = "Download aborted.";
                    return;
                }

                button.Text = "Start downloading ...";

                foo.StartDownloading ();

                foo.File.PropertyChanged += (sender, e) => {
                    // Update UI if download-status changed.
                    if (e.PropertyName == "Status") {
                        switch (((IDownloadFile)sender).Status) {
                        case DownloadFileStatus.COMPLETED:
                        case DownloadFileStatus.FAILED:
                        case DownloadFileStatus.CANCELED:
                            button.Text = "Downloading finished.";

                            // Get the path this file was saved to. When you didn't set a custom path, this will be some temporary directory.
                            var nativeDownloadManager = (DownloadManager)ApplicationContext.GetSystemService (DownloadService);
                            System.Diagnostics.Debug.WriteLine (nativeDownloadManager.GetUriForDownloadedFile (((DownloadFileImplementation)sender).Id));
                            break;
                        }
                    }

                    // Update UI while donwloading.
                    if (e.PropertyName == "TotalBytesWritten" || e.PropertyName == "TotalBytesExpected") {
                        var bytesExpected = ((IDownloadFile)sender).TotalBytesExpected;
                        var bytesWritten = ((IDownloadFile)sender).TotalBytesWritten;

                        if (bytesExpected > 0) {
                            var percentage = Math.Round (bytesWritten / bytesExpected * 100);
                            button.Text = "Downloading (" + percentage + "%)";
                        }
                    }
                };
            };
        }
    }
}


