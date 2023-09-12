using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;

namespace cloudfare_bypass_cefsharp
{
	public class LifeSpanHandler : ILifeSpanHandler
	{
		public LifeSpanHandler(bool _isblockPopup = true, string _userangent = "")
		{
			IsblockPopup = _isblockPopup;
			UserAngent = _userangent;
		}

		public string UserAngent   { get; set; }
		public bool   IsblockPopup { get; set; }

		public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
		{
			return false;
		}

		public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser)
		{
			if (!string.IsNullOrEmpty(UserAngent))
			{
				using (var client = chromiumWebBrowser.GetDevToolsClient())
				{
					_ = client.Network.SetUserAgentOverrideAsync(userAgent: UserAngent);
				}
			}
		}

		public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
		{
			//     throw new NotImplementedException();
		}

		public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl,
								  string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture,
								  IPopupFeatures popupFeatures, IWindowInfo windowInfo,
								  IBrowserSettings browserSettings, ref bool noJavascriptAccess,
								  out IWebBrowser newBrowser)
		{
			/*newBrowser = new ChromiumWebBrowser("https://google.com");
return false;*/

			newBrowser = null;

			//Use IWindowInfo.SetAsChild to specify the parent handle
			//NOTE: user PopupAsChildHelper to handle with Form move and Control resize

			return false;
		}
	}
}
