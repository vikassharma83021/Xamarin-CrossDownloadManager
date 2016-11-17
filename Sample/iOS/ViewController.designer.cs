// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace DownloadExample.iOS
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		UIKit.UIButton Button { get; set; }

		[Outlet]
		UIKit.UITextField StatusDetailsTextField { get; set; }

		[Outlet]
		UIKit.UITextField StatusTextField { get; set; }

		[Outlet]
		UIKit.UISwitch Switch { get; set; }

		[Outlet]
		UIKit.UITextField TotalBytesExpectedTextField { get; set; }

		[Outlet]
		UIKit.UITextField TotalBytesWrittenTextField { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (Button != null) {
				Button.Dispose ();
				Button = null;
			}

			if (Switch != null) {
				Switch.Dispose ();
				Switch = null;
			}

			if (StatusTextField != null) {
				StatusTextField.Dispose ();
				StatusTextField = null;
			}

			if (StatusDetailsTextField != null) {
				StatusDetailsTextField.Dispose ();
				StatusDetailsTextField = null;
			}

			if (TotalBytesExpectedTextField != null) {
				TotalBytesExpectedTextField.Dispose ();
				TotalBytesExpectedTextField = null;
			}

			if (TotalBytesWrittenTextField != null) {
				TotalBytesWrittenTextField.Dispose ();
				TotalBytesWrittenTextField = null;
			}
		}
	}
}
