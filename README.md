## CrossDownloadManager

The CrossDownloadManager is a plugin that helps you downloading files in the background.

### Build Status: 
[![Build status](https://ci.appveyor.com/api/projects/status/c9c6recwcu7k0s15?svg=true)](https://ci.appveyor.com/project/SimonSimCity/xamarin-crossdownloadmanager)
![GitHub tag](https://img.shields.io/github/tag/SimonSimCity/xamarin-crossdownloadmanager.svg)
[![NuGet](https://img.shields.io/nuget/v/Xam.Plugins.DownloadManager.svg?label=NuGet)](https://www.nuget.org/packages/Xam.Plugins.DownloadManager/)
[![MyGet](https://img.shields.io/myget/simonsimcity/vpre/Xam.Plugins.DownloadManager.svg)](https://www.myget.org/F/simonsimcity/api/v2)

### Where can I use it?

|Platform|Supported|Version|
| ------------------- | :-----------: | :------------------: |
|Xamarin.iOS|Yes|iOS 7+|
|Xamarin.iOS Unified|Yes|iOS 7+|
|Xamarin.Android|Yes|API 16+|
|Windows 10 UWP|Yes|10.0.10240.0|
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

### Start downloading

You can now start a download by adding the following code:
```
    var downloadManager = CrossDownloadManager.Current;
    var file = downloadManager.CreateDownloadFile(url);
    downloadManager.Start(file);
```

This will add the file to a native library, which starts the download of that file. You can watch the properties of the `IDownloadFile` instance and execute some code if e.g. the status changes to `COMPLETED`, you can also watch the `IDownloadManager.Queue` and execute some code if the list of files, that will be downloaded or are currently downloading changes.

After a download has been completed, the instance of `IDownloadFile` is then removed from `IDownloadManager.Queue`.

You can also disallow downloading via a cellular network by setting the second parameter of `CrossDownloadManager.Current.Start()`.

### Where are the files stored?

#### Default Option - Temporary Location

When you choose not to provide your own path before starting the download, the downloaded files are stored at a temporary directory and may be removed by the OS e.g. when the system runs out of space. You can move this file to a decided destination by listening on whether the status of the files changes to `DownloadFileStatus.COMPLETED`. You can find an implementation in the sample: https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/27

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

On Android, the destination location must be a located outside of your Apps internal directory (see [#10](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/10) for details). To allow your app to write to that location, you either have to add the permission `WRITE_EXTERNAL_STORAGE` to the mainfest.xml file to require it when installing the app
```
    <uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
```

or to request it at runtime (See [#20](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/20)).

All finished downloads are registered in a native `Downloads` application. If you want your finished download not to be listed there, see [#17](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/17)

### I want to use $FAVORITE_IOC_LIBRARY

Just register the instance in `CrossDownloadManager.Current` in the library. Here's an example how to do it on MvvmCross:

    Mvx.RegisterSingleton<IDownloadManager>(() => CrossDownloadManager.Current);

### Can I just have a look at a sample implementation?

I've created a quite basic implementation for UWP, iOS and Android. You can find it in the folder "Sample" in this repository.

### Contribute

If you want to contribute, just fork the project, write some code or just file an issue if you don't know how to realize the change you want to see.

### Licensing

[This plugin is licensed under the MIT License](https://opensource.org/licenses/MIT)

### Contributors

* [SimonSimCity](https://github.com/SimonSimCity)
* [martijn00](https://github.com/martijn00)
* [fela98](https://github.com/fela98)
* [BtrJay](https://github.com/BtrJay)

### Changes

#### 1.3.1

  * Fixed bug [#64](https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/issues/64): Cancel button on Notification does not change file status to `DownloadFileStatus.CANCELED`

#### 1.3.0

  * Fixed download-queue on UWP
  * Added option for Android to disable notification

#### 1.2.0

  * Added property `CrossDownloadManager.AvoidDiscretionaryDownloadInBackground` to avoid setting `discretionary` to `true` if download starts while app is in background (see https://github.com/SimonSimCity/Xamarin-CrossDownloadManager/pull/50/commits/bd94398623043cd3e29f743201302103d9d48ba1#diff-a606a1aeb07f8c9297bce51968abc4c6R32 for details)
  * Fixed calling of DownloadCompletedBroadcastReceiver in background by adding an annotation (and thereby dropped requirement to register class ActivityLifecycleCallbacks)

#### 1.1.1

  * Fixed automated build-process - nothing functional to this plugin ...

#### 1.1.0

  * Added first implementation for UWP
  * [BC] Updated naming of methods to follow C# guidelines

#### 1.0.0

  * Allowed implementation of Mac (without crashing at once)

#### 0.10.2

  * Fixed NullReferenceException

#### 0.10.1

  * Small fixes

#### 0.10.0

  * [BC] Introduced new status in DownloadFileStatus enum. Please check your switch-case statements on `DownloadFileStatus`. Be aware of that items can also be in the `Queue` while still having the status `DownloadFileStatus.INITIALIZED`.
  * [BC] Refactored queue to be threadsafe. `Queue` is implemented as `IEnumerable<T>` and the `CollectionChanged` listener is implemented on the `CrossDownloadManager` itself.

#### 0.9.1

  * Small fixes

#### 0.9.0

  * [Bug] Updated session-identifier on iOS. (If another app is running, using the same extension, you don't have accesss to their downloads anymore)
  * [Feature] Option to disallow downloading via mobile network
  * [Bug] Fixed bug on iOS when reestablishing the queue after the app was shut down
