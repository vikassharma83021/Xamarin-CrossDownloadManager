using System;
#if __ANDROID__
using Android.App;
#endif
using Plugin.DownloadManager.Abstractions;

namespace Plugin.DownloadManager
{
    /// <summary>
    /// Cross platform DownloadManager implemenations
    /// </summary>
    public class CrossDownloadManager
    {
        static Lazy<IDownloadManager> Implementation = new Lazy<IDownloadManager> (() => CreateDownloadManager (), System.Threading.LazyThreadSafetyMode.PublicationOnly);

#if __IOS__
        public static Action BackgroundSessionCompletionHandler;
        public static UrlSessionDownloadDelegate UrlSessionDownloadDelegate;
#endif

        /// <summary>
        /// Current settings to use
        /// </summary>
        public static IDownloadManager Current {
            get {
                var ret = Implementation.Value;
                if (ret == null) {
                    throw NotImplementedInReferenceAssembly ();
                }
                return ret;
            }
        }

#if __ANDROID__
        public static void Init (Activity activity)
        {
            ActivityLifecycleCallbacks.Register (activity, () => Current);
        }
#endif

        static IDownloadManager CreateDownloadManager ()
        {
#if __IOS__
            return new DownloadManagerImplementation (UrlSessionDownloadDelegate ?? new iOS.UrlSessionDownloadDelegate());
#elif __ANDROID__
            return new DownloadManagerImplementation(ActivityLifecycleCallbacks.CurrentTopActivity);
#else
            return null;
#endif
        }

        internal static Exception NotImplementedInReferenceAssembly ()
        {
            return new NotImplementedException ("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
        }
    }
}
