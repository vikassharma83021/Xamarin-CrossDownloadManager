## CrossDownloadManager

The CrossDownloadManager is a plugin that helps you downloading files in the background.

### Get started

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

### MvvmCross

    Mvx.RegisterSingleton<IDownloadManager>(() => CrossDownloadManager.Instance);

### Start downloading

You can now start a download by adding the following code:
```
    var downloadManager = CrossDownloadManager.Instance;
    var file = downloadManager.CreateDownloadFile(url);
    downloadManager.Start(file);
```

This will add the file to a native library, which starts the download of that file. You can watch the properties of the `IDownloadFile` instance and execute some code if e.g. the status changes to `COMPLETED`, but you can also watch the `IDownloadManager.Queue` and execute some code if the list of files, that will be downloaded or are currently downloading changes.

After a download has completed, it is removed from `IDownloadManager.Queue`.

### Contribute

If you want to contribute, just fork the project, write some code or just file an issue if you don't konw how to realize the change you want to see.

### Licensing

[This plugin is licensed under the MIT License](https://opensource.org/licenses/MIT)