## CrossDownloadManager

The CrossDownloadManager is a plugin that helps you downloading files in the background.

### Where can I use it?

The CrossDownloadManager is currently compatible with:

* Xamarin.iOS
* Xamarin.Android

### Getting started

Add the nuget package to your cross-platform project and to every platform specific project. Now, you have to initialize the service for every platform. You also need to write some logic, which determines where the file will be saved.

#### iOS

_AppDelegate.cs_
```
    /**
     * Save the completion-handler we get when the app starts back from the background.
     * This method informs iOS that the app has finished all internal processing and can sleep again.
     */
    public override void HandleEventsForBackgroundUrl(UIApplication application, string sessionIdentifier, Action completionHandler)
    {
        CrossDownloadManager.BackgroundSessionCompletionHandler = completionHandler;
    }
```

As of iOS 9, your URL MUST be secured or you have to add the domain to the list of exceptions. See [https://developer.apple.com/library/ios/releasenotes/General/WhatsNewIniOS/Articles/iOS9.html#//apple_ref/doc/uid/TP40016198-SW14](https://developer.apple.com/library/ios/releasenotes/General/WhatsNewIniOS/Articles/iOS9.html#//apple_ref/doc/uid/TP40016198-SW14)
### Android

Depending on the location you want the file to be saved, you may have to ask for the permission `WRITE_EXTERNAL_STORAGE`:
```
    <uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
``` 

_Activity.cs_
```
    protected override void OnCreate ()
    {
        [...]
        CrossDownloadManager.Init (this);
    }
```

### Start downloading

You can now start a download by adding the following code:
```
    var downloadManager = CrossDownloadManager.Current;
    var file = downloadManager.CreateDownloadFile(url);
    downloadManager.Start(file);
```

This will add the file to a native library, which starts the download of that file. You can watch the properties of the `IDownloadFile` instance and execute some code if e.g. the status changes to `COMPLETED`, but you can also watch the `IDownloadManager.Queue` and execute some code if the list of files, that will be downloaded or are currently downloading changes.

After a download has completed, it is removed from `IDownloadManager.Queue`.

### Where are the files stored?

Usually, you would expect to set the path to the `IDownloadFile` instance, you get when calling `downloadManager.CreateDownloadFile(url)`. But, as this background-downloader even continues when the app crashed, you have to be able to reconstruct the path in every stage of the app. The correct way is to register a method as early as possible, that, in every circumstance, can reconstruct the path the file should be saved. This code line could look like following:
```
    CrossDownloadManager.Current.PathNameForDownloadedFile = new System.Func<IDownloadFile, string> (file => {
#if __IOS__
            string fileName = (new NSUrl(file.Url, false)).LastPathComponent;
            return Path.Combine(Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), fileName);
#elif __ANDROID__
            string fileName = Android.Net.Uri.Parse(file.Url).Path.Split('/').Last();
            return Path.Combine (ApplicationContext.GetExternalFilesDir (Android.OS.Environment.DirectoryDownloads).AbsolutePath, fileName);
#else
            string fileName = '';
            return Path.Combine(Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), fileName);
#endif
        });
```

Please be aware of that the destination on Android MUST be on an external storage. See [#10](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/10)

### I want to use $FAVORITE_IOC_LIBRARY

Just register the instance in `CrossDownloadManager.Current` in the library. Here's an example how to do it on MvvmCross:

    Mvx.RegisterSingleton<IDownloadManager>(() => CrossDownloadManager.Current);

### Contribute

If you want to contribute, just fork the project, write some code or just file an issue if you don't know how to realize the change you want to see.

### Licensing

[This plugin is licensed under the MIT License](https://opensource.org/licenses/MIT)
