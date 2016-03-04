using System;
using Android.App;
using Android.Content;
using Android.OS;
using Plugin.DownloadManager.Abstractions;

namespace Plugin.DownloadManager
{
    public class ActivityLifecycleCallbacks : Java.Lang.Object, Application.IActivityLifecycleCallbacks
    {
        DownloadCompletedBroadcastReceiver _receiverDownoladCompleted;

        Func<IDownloadManager> _downloadManagerAction;

        public static Activity CurrentTopActivity { get; protected set; }

        public static void Register (Activity activity, Func<IDownloadManager> downloadManagerAction)
        {
            activity.Application.RegisterActivityLifecycleCallbacks (new ActivityLifecycleCallbacks (downloadManagerAction));
            CurrentTopActivity = activity;
        }

        public ActivityLifecycleCallbacks (Func<IDownloadManager> downloadManagerAction)
        {
            _downloadManagerAction = downloadManagerAction;
        }

        public virtual void OnActivityCreated (Activity activity, Bundle savedInstanceState)
        {
            CurrentTopActivity = activity;
        }

        public virtual void OnActivityDestroyed (Activity activity) { }

        public virtual void OnActivityPaused (Activity activity)
        {
            activity.UnregisterReceiver (_receiverDownoladCompleted);
        }

        public virtual void OnActivityResumed (Activity activity)
        {
            CurrentTopActivity = activity;

            _receiverDownoladCompleted = new DownloadCompletedBroadcastReceiver (_downloadManagerAction);
            activity.RegisterReceiver (
                _receiverDownoladCompleted,
                new IntentFilter (Android.App.DownloadManager.ActionDownloadComplete)
            );
        }

        public virtual void OnActivitySaveInstanceState (Activity activity, Bundle outState) { }

        public virtual void OnActivityStarted (Activity activity) { }

        public virtual void OnActivityStopped (Activity activity) { }
    }
}
