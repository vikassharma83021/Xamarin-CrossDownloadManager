using Plugin.DownloadManager.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.Content;

namespace Plugin.DownloadManager
{
  /// <summary>
  /// Implementation for Feature
  /// </summary>
  public class DownloadManagerImplementation : IDownloadManager
  {
		Android.App.DownloadManager _downloadManager;

		public ObservableCollection<IDownloadFile> Queue { get; private set; }

		public Func<IDownloadFile, string> UriForDownloadedFile { get; set; }

		public DownloadManagerImplementation (Context applicationContext)
		{
			Queue = new ObservableCollection<IDownloadFile> ();

			_downloadManager = (Android.App.DownloadManager)applicationContext.GetSystemService (Context.DownloadService);
		}

		public IDownloadFile CreateDownloadFile (string url, IDictionary<string, string> headers)
		{
			return new DownloadFile (url, headers);
		}

		public void Start (IDownloadFile i)
		{
			var file = (DownloadFile)i;

			file.StartDownload (_downloadManager, UriForDownloadedFile (file));
			Queue.Add (file);
		}

		public void Abort (IDownloadFile i)
		{
			var file = (DownloadFile)i;

			file.Status = DownloadStatus.CANCELED;
			_downloadManager.Remove (file.Id);
			Queue.Remove (file);
		}

		public void AbortAll ()
		{
			foreach (var file in Queue) {
				Abort (file);
			}
		}
    }
}