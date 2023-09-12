using System.Threading.Tasks;
using CefSharp;
using Nito.AsyncEx;

namespace cloudfare_bypass_cefsharp
{
	internal class RenderProcessMessageHandler : IRenderProcessMessageHandler
	{
		private TaskCompletionSource _contextCreated;

		// Wait for the underlying JavaScript Context to be created. This is only called for the main frame.
		// If the page has no JavaScript, no context will be created.
		void IRenderProcessMessageHandler.OnContextCreated(IWebBrowser browserControl, IBrowser browser, IFrame frame)
		{
			//const string script = "document.addEventListener('DOMContentLoaded', function(){ alert('DomLoaded'); });";

			//frame.ExecuteJavaScriptAsync(script);
			if (_contextCreated == null)
			{
				_contextCreated = new TaskCompletionSource();
			}

			if (!_contextCreated.Task.IsCompleted)
			{
				_contextCreated.TrySetResult();
			}
		}

		public void OnContextReleased(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
		{
		}

		public void OnFocusedNodeChanged(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IDomNode node)
		{
		}

		public void OnUncaughtException(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
										JavascriptException exception)
		{
		}
	}
}