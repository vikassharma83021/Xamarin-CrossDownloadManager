## CrossDownloadManager

The CrossDownloadManager is a plugin that helps you downloading files in the background.

### Where can I use it?

|Platform|Supported|Version|
| ------------------- | :-----------: | :------------------: |
|Xamarin.iOS|Yes|iOS 7+|
|Xamarin.iOS Unified|Yes|iOS 7+|
|Xamarin.Android|Yes|API 14+|
|Windows Phone Silverlight|No||
|Windows Phone RT|No||
|Windows Store RT|No||
|Windows 10 UWP|No - Requested (#8)||
|Xamarin.Mac|No||

### Getting started

Add the nuget package to your cross-platform project and to every platform specific project. Now, you have to initialize the service for every platform. You also need to write some logic, which determines where the file will be saved.

#### iOS

_AppDelegate.cs_
```
    /**
     * Save the completion-handler we get when the app opens from the background.
     * This method informs iOS that the app has finished all internal processing and can sleep again.
     */
    public override void HandleEventsForBackgroundUrl(UIApplication application, string sessionIdentifier, Action completionHandler)
    {
        CrossDownloadManager.BackgroundSessionCompletionHandler = completionHandler;
    }
```

As of iOS 9, your URL must be secured or you have to add the domain to the list of exceptions. See [https://developer.apple.com/library/ios/releasenotes/General/WhatsNewIniOS/Articles/iOS9.html#//apple_ref/doc/uid/TP40016198-SW14](https://developer.apple.com/library/ios/releasenotes/General/WhatsNewIniOS/Articles/iOS9.html#//apple_ref/doc/uid/TP40016198-SW14)
#### Android

_MainApplication.cs_
```
    Plugin.DownloadManager.ActivityLifecycleCallbacks _downloadManagerLifecycleCallbacks;

    public MainApplication(IntPtr handle, JniHandleOwnership transer)
      :base(handle, transer)
    {
        [...]
        _downloadManagerLifecycleCallbacks = new Plugin.DownloadManager.ActivityLifecycleCallbacks();
    }

    public override void OnCreate()
    {
        [...]
        RegisterActivityLifecycleCallbacks(_downloadManagerLifecycleCallbacks);
    }

    public override void OnTerminate()
    {
        [...]
        UnregisterActivityLifecycleCallbacks(_downloadManagerLifecycleCallbacks);
    }
```

### Start downloading

You can now start a download by adding the following code:
```
    var downloadManager = CrossDownloadManager.Current;
    var file = downloadManager.CreateDownloadFile(url);
    downloadManager.Start(file);
```

This will add the file to a native library, which starts the download of that file. You can watch the properties of the `IDownloadFile` instance and execute some code if e.g. the status changes to `COMPLETED`, you can also watch the `IDownloadManager.Queue` and execute some code if the list of files, that will be downloaded or are currently downloading changes.

After a download has been completed, the instance of `IDownloadFile` is then removed from `IDownloadManager.Queue`.

### Where are the files stored?

#### Default Option - Temperory Location

When you choose not to provide your own path before starting the download, the downloaded files are stored at a temporary directory and may be removed by the OS e.g. when the system runs out of space. You can move this file to a decided destination by listening on whether the status of the files changes to `DownloadFileStatus.COMPLETED`. You can find an implementation in the sample: https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/commit/14b67698fe8488cb95351a933590dc934f6cf4e3

#### Recommended Option - Custom Location

Usually, you would expect to set the path to the `IDownloadFile` instance, you get when calling `downloadManager.CreateDownloadFile(url)`. But, as this download manager even continues downloading when the app crashed, you have to be able to reconstruct the path in every stage of the app. The correct way is to register a method as early as possible, that, in every circumstance, can reconstruct the path that the file should be saved. This method could look like following:
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
##### Additional for Andriod

On Android, the destination location must be a located outside of your Apps internal directory (see [#10](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/10) for details). To allow your app to write to that location, you will need the the permission `WRITE_EXTERNAL_STORAGE` (See [Android API for DownloadManager.Request.setDestinationUri()](https://developer.android.com/reference/android/app/DownloadManager.Request.html#setDestinationUri%28android.net.Uri%29):
```
    <uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
``` 


### I want to use $FAVORITE_IOC_LIBRARY

Just register the instance in `CrossDownloadManager.Current` in the library. Here's an example how to do it on MvvmCross:

    Mvx.RegisterSingleton<IDownloadManager>(() => CrossDownloadManager.Current);

### Can I just have a look at a sample implementation?

I've created a quite basic implementation for iOS and Android. You can find it in the folder "Sample" in this repository.

### Contribute

If you want to contribute, just fork the project, write some code or just file an issue if you don't know how to realize the change you want to see.

### Licensing

[This plugin is licensed under the MIT License](https://opensource.org/licenses/MIT)
