using Android.App;
using Android.Content;
using Android.OS;

namespace Plugin.DownloadManager
{
    public class ActivityLifecycleCallbacks : Java.Lang.Object, Application.IActivityLifecycleCallbacks
    {
        DownloadCompletedBroadcastReceiver _receiverDownoladCompleted;

        public virtual void OnActivityCreated (Activity activity, Bundle savedInstanceState) { }

        public virtual void OnActivityDestroyed (Activity activity) { }

        public virtual void OnActivityPaused (Activity activity)
        {
            activity.UnregisterReceiver (_receiverDownoladCompleted);
        }

        public virtual void OnActivityResumed (Activity activity)
        {
            _receiverDownoladCompleted = new DownloadCompletedBroadcastReceiver ();
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
