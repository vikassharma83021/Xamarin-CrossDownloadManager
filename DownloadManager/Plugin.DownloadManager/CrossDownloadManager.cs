using Plugin.DownloadManager.Abstractions;
using System;

namespace Plugin.DownloadManager
{
  /// <summary>
  /// Cross platform DownloadManager implemenations
  /// </summary>
  public class CrossDownloadManager
  {
    static Lazy<IDownloadManager> Implementation = new Lazy<IDownloadManager>(() => CreateDownloadManager(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

    /// <summary>
    /// Current settings to use
    /// </summary>
    public static IDownloadManager Current
    {
      get
      {
        var ret = Implementation.Value;
        if (ret == null)
        {
          throw NotImplementedInReferenceAssembly();
        }
        return ret;
      }
    }

    static IDownloadManager CreateDownloadManager()
    {
#if PORTABLE
        return null;
#else
        return new DownloadManagerImplementation();
#endif
    }

    internal static Exception NotImplementedInReferenceAssembly()
    {
      return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
    }
  }
}
