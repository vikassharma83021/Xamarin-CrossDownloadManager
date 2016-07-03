using System;

using Android.App;
using Android.Runtime;

namespace DownloadExample.Droid
{
    [Application]
    public class MainApplication : Application
    {
        Plugin.DownloadManager.ActivityLifecycleCallbacks _downloadManagerLifecycleCallbacks;

        public MainApplication(IntPtr handle, JniHandleOwnership transer)
          :base(handle, transer)
        {
            _downloadManagerLifecycleCallbacks = new Plugin.DownloadManager.ActivityLifecycleCallbacks();
        }

		public override void OnCreate()
        {
            base.OnCreate();
            RegisterActivityLifecycleCallbacks(_downloadManagerLifecycleCallbacks);
        }

        public override void OnTerminate()
        {
            base.OnTerminate();
            UnregisterActivityLifecycleCallbacks(_downloadManagerLifecycleCallbacks);
        }
	}
}