using CefSharp;
using System.Security.Cryptography.X509Certificates;

namespace cloudfare_bypass_cefsharp
{
	public class RequestHandel : IRequestHandler
	{
		private string customUserAngent;

			public RequestHandel(string userangent = null)
			{
				customUserAngent = userangent;
			}

			public bool CanGetCookies(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
			{
				return true;
			}

			public bool CanSetCookie(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request,
									 Cookie cookie)
			{
				return true;
			}

			public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, bool isProxy,
										   string host, int port, string realm, string scheme, IAuthCallback callback)
			{
				return false;
			}

			public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl,
										   bool isProxy, string host, int port, string realm, string scheme,
										   IAuthCallback callback)
			{
				throw new System.NotImplementedException();
			}

			public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser,
																	 IFrame frame, IRequest request, bool isNavigation,
																	 bool isDownload, string requestInitiator,
																	 ref bool disableDefaultHandling)
			{
				return null;
			}

			public IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser,
															 IFrame frame, IRequest request, IResponse response)
			{
				return null;
			}

			public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request,
									   bool userGesture, bool isRedirect)
			{
				return false;
			}

			public CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
													   IRequest request, IRequestCallback callback)
			{
				if (!string.IsNullOrEmpty(customUserAngent))
				{
					var headers = request.Headers;


					headers.Remove("HTTP_FORWARDED");
					headers.Remove("REAL_IP");
					headers.Remove("HTTP_ACCEPT");
					headers.Remove("X_REAL_IP");
					headers.Remove("X-Forwarded-For");

					headers.Remove("ACCEPT");
					request.Headers = headers;

					request.SetHeaderByName("user-agent", customUserAngent, true);
				}

				return CefReturnValue.Continue;
			}

			public bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode,
										   string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
			{
				return false;
			}

			public void OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser)
			{
			}

			public bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
										 string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
			{
				return false;
			}

			public void OnPluginCrashed(IWebBrowser chromiumWebBrowser, IBrowser browser, string pluginPath)
			{
			}

			public bool OnProtocolExecution(IWebBrowser chromiumWebBrowser, IBrowser browser, string url)
			{
				return false;
			}

			public bool OnQuotaRequest(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, long newSize,
									   IRequestCallback callback)
			{
				return false;
			}

			public void OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser,
												  CefTerminationStatus status)
			{
			}

			public void OnRenderViewReady(IWebBrowser chromiumWebBrowser, IBrowser browser)
			{
			}

			public void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
											   IRequest request, IResponse response, UrlRequestStatus status,
											   long receivedContentLength)
			{
			}

			public void OnResourceRedirect(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
										   IRequest request, IResponse response, ref string newUrl)
			{
			}

			public bool OnResourceResponse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
										   IRequest request, IResponse response)
			{
				return false;
			}

			public bool OnSelectClientCertificate(IWebBrowser chromiumWebBrowser, IBrowser browser, bool isProxy,
												  string host, int port, X509Certificate2Collection certificates,
												  ISelectClientCertificateCallback callback)
			{
				return false;
			}
		}
}