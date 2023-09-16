using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.DevTools.Input;
using CefSharp.Handler;
using CefSharp.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Point = System.Drawing.Point;

namespace cloudfare_bypass_cefsharp
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			DefaultUserAngent =
				"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.122 Safari/537.36";
		}

		public string DefaultUserAngent { get; set; }


		// This will setup default one time for all ChromiumWebBrowser
		public async Task SetupCef(bool isheadless = false)
		{
			var settings = new CefSettings();
			settings.MultiThreadedMessageLoop = true;
			if (isheadless)
			{
				settings.WindowlessRenderingEnabled = true;
			}


			settings.UserAgent = DefaultUserAngent;
			settings.AcceptLanguageList = "en-US;q=0.8,en;q=0.7";


			// You can setting the cache path to allow cefsharp store cookies in folder,
			// if not setting cefsharp will store cookies in memory, and lost when close application
			var pathcef = Path.Combine(Application.StartupPath, "cefdata");
			if (!Directory.Exists(pathcef))
			{
				Directory.CreateDirectory(pathcef);
			}

			settings.CachePath = pathcef;
			// Initialize cef with the provided settings
			CefSharpSettings.SubprocessExitIfParentProcessClosed = true;
			CefSharpSettings.FocusedNodeChangedEnabled = true;
			CefSharpSettings.ShutdownOnExit = true;
			settings.CefCommandLineArgs.Add("disable-web-security");
			settings.CefCommandLineArgs.Add("disable-application-cache");


			Cef.Initialize(settings, performDependencyCheck: true,
				browserProcessHandler: new BrowserProcessHandler());

			// Disable Web RTC Default
			await Cef.UIThreadTaskFactory.StartNew(() =>
			{
				string errors;
				var rc = Cef.GetGlobalRequestContext();
				rc.SetPreference("webrtc.multiple_routes_enabled", false, out errors);
				rc.SetPreference("webrtc.nonproxied_udp_enabled", false, out errors);

				rc.SetPreference("webrtc.ip_handling_policy", "disable_non_proxied_udp", out errors);

				var b = rc.HasPreference("webrtc");

				var c = rc.GetAllPreferences(true);
			});
		}

		ChromiumWebBrowser ChromiumWeb;


		private async void button1_Click(object sender, EventArgs e)
		{
			// setting userangent for cef
			// you can setting this for each ChromiumWebBrowser, this will override the default setting
			var UserAngent =
				"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.5304.122 Safari/537.36";


			RequestContextSettings requestContextSettings = new RequestContextSettings
			{
			};

			IRequestContext requestContext = new RequestContext(requestContextSettings);

			ChromiumWeb = new ChromiumWebBrowser("https://www.yell.com/s/acrylics-aberdeen.html", requestContext);
			ChromiumWeb.Dock = DockStyle.Fill;
			panel1.Controls.Add(ChromiumWeb);
			ChromiumWeb.ActivateBrowserOnCreation = true;
			ChromiumWeb.RequestHandler = new RequestHandel(UserAngent);

			ChromiumWeb.RenderProcessMessageHandler = new RenderProcessMessageHandler();

			ChromiumWeb.LifeSpanHandler = new LifeSpanHandler { IsblockPopup = true, UserAngent = UserAngent };

			ChromiumWeb.LoadingStateChanged += ChromiumWeb_LoadingStateChanged;

			ChromiumWeb.ActivateBrowserOnCreation = true;

			// wait for captcha frame display, you also can check the captcha frame is display by check the html source code  via javascript
			// this just a example, you can use your own logic to check the captcha frame is display for fastter click, no need wait until 10 seconds
			await Task.Delay(10000);

			// you can use this code to show dev tool, easy for tartget element in browser
			//ChromiumWeb.ShowDevTools();

			var lisframe =
				ChromiumWeb.GetBrowser().GetFrameNames();

			string f = "";
			foreach (var VARIABLE in lisframe)
			{
				if (!string.IsNullOrEmpty(VARIABLE))
				{
					f = VARIABLE;
					break;
				}
			}

			// Get the iframe's position relative to the browser
			Point? p1 = await GetCordFrameInBroswer(f);

			// Get the element's position relative to the ifame
			////the  old code is image[class*="mark"]

			//Point? p2 = await GetCordElInFrame(f,
			//	"img[class*=\"mark\"]");

			//// new code
			Point? p2 = await GetCordElInFrame(f,
				"span[class*=\"mark\"]");
			Point newp = new Point(p1.Value.X + p2.Value.X, p2.Value.Y + p1.Value.Y);
			var inprt = ChromiumWeb.GetBrowser().GetHost().GetWindowHandle();

			// This click method through cpd chrome
			await ClickAsync(newp.X, newp.Y,
				new ClickOptions { Button = MouseButton.Left, ClickCount = 1, Delay = 200 });
		}


		private bool IsLoading = false;

		private void ChromiumWeb_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
		{
			if (!e.IsLoading)
			{
				IsLoading = false;
			}
			else
			{
				IsLoading = true;
			}
		}


		public async Task<Point?> GetCordElInFrame(string framename, string attr)
		{
			Point point = Point.Empty;
			IFrame f = ChromiumWeb.GetBrowser().GetFrame(framename);
			if (!f.IsValid)
			{
				return null;
			}
			//

			var script = @"(function () {
			    var bnt = document.querySelector('" + attr + @"');
			   
			    var bntRect = bnt.getBoundingClientRect();
			    return JSON.stringify({ x: bntRect.left, y: bntRect.top });
			})();";
			JavascriptResponse jsReponse = await f.EvaluateScriptAsync(
				script
			);
			if (jsReponse.Success && jsReponse.Result != null)
			{
				string jsonString = (string)jsReponse.Result;

				if (jsonString != null)
				{
					JObject jsonObject = JObject.Parse(jsonString);
					int xPosition = (int)jsonObject["x"] + 1; // add +1 pixel to the click position
					int yPosition = (int)jsonObject["y"] + 1; // add +1 pixel to the click position

					point = new Point(xPosition, yPosition);
				}
			}

			return point;
		}

		public async Task<Point?> GetCordFrameInBroswer(string framename)
		{
			Point point = Point.Empty;
			IFrame f = ChromiumWeb.GetBrowser().GetFrame(framename);
			if (!f.IsValid)
			{
				return null;
			}

			int x = 0;
			int y = 0;
			IFrame fr = null;
			do
			{
				if (fr == null)
				{
					if (f.IsMain)
					{
						fr = f;
					}
					else
					{
						fr = f.Parent;
					}
				}
				else
				{
					fr = fr.Parent;
				}

				JavascriptResponse jsre = await fr.EvaluateScriptAsync(
					@"(function () {
			    var bnt = document.getElementsByTagName('iframe')[0];

			    var bntRect = bnt.getBoundingClientRect();
			    return JSON.stringify({ x: bntRect.left, y: bntRect.top });
				})();"
				);

				if (jsre.Success && jsre.Result != null)
				{
					string jsonString = (string)jsre.Result;

					if (jsonString != null)
					{
						JObject jsonObject = JObject.Parse(jsonString);
						x += (int)jsonObject["x"] + 1; // add +1 pixel to the click position
						y += (int)jsonObject["y"] + 1; // add +1 pixel to the click position
					}
				}

				if (fr.IsMain)
				{
					break;
				}
			} while (true);

			point = new Point(x, y);

			return point;
		}

		private int LastMessageId = 6000;

		public async Task SendDevtoolEvent(string evnt, object eventnameAndParam)
		{
			Dictionary<string, object> param = new Dictionary<string, object>();

			if (eventnameAndParam != null)
			{
				var json = JsonConvert.SerializeObject(eventnameAndParam, Formatting.Indented,
					new JsonSerializerSettings
					{
						ContractResolver = new LowercaseContractResolver()
					});
				param = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
			}

			IBrowserHost host = ChromiumWeb.GetBrowser().GetHost();
			if (host == null || host.IsDisposed)
			{
				throw new Exception("BrowserHost is Null or Disposed");
			}


			int msgId = Interlocked.Increment(ref LastMessageId);

			TaskMethodDevToolsMessageObserver observer = new TaskMethodDevToolsMessageObserver(msgId);

			//Make sure to dispose of our observer registration when done
			//TODO: Create a single observer that maps tasks to Id's
			//Or at least create one for each type, events and method
			using (IRegistration observerRegistration = host.AddDevToolsMessageObserver(observer))
			{
				//Page.captureScreenshot defaults to PNG, all params are optional
				//for this DevTools method
				int id = 0;

				//TODO: Simplify this, we can use an Func to reduce code duplication
				if (Cef.CurrentlyOnThread(CefThreadIds.TID_UI))
				{
					id = host.ExecuteDevToolsMethod(msgId, evnt, param);
				}
				else
				{
					id = await Cef.UIThreadTaskFactory.StartNew(() =>
					{
						var json = JsonConvert.SerializeObject(param);
						//	return host.ExecuteDevToolsMethod(msgId, evnt, param);
						return host.ExecuteDevToolsMethod(msgId, evnt, json);
					});
				}

				if (id != msgId)
				{
					throw new Exception("Message Id doesn't match the provided Id");
				}

				Tuple<bool, byte[]> result = await observer.Task.ConfigureAwait(false);

				bool success = result.Item1;

				dynamic response = JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(result.Item2));
				string res = JsonConvert.SerializeObject(response);

				//Success
				if (success)
				{
				}

				string code = (string)response.code;
				string message = (string)response.message;
			}
		}

		private int msgId;

		public async Task MoveAsync(decimal x, decimal y, MoveOptions options = null)
		{
			options = options ?? new MoveOptions();
			var fromX = _x;
			var fromY = _y;
			_x = x;
			_y = y;
			var steps = options.Steps;

			for (var i = 1; i <= steps; i++)
			{
				await SendDevtoolEvent("Input.dispatchMouseEvent", new InputDispatchMouseEventRequest
				{
					Type = MouseEventType.MouseMoved,
					Button = "left",
					X = fromX + ((_x - fromX) * ((decimal)i / steps)),
					Y = fromY + ((_y - fromY) * ((decimal)i / steps)),
					Modifiers = 0
				}).ConfigureAwait(false);
			}
		}

		private decimal     _x      = 0;
		private decimal     _y      = 0;
		private MouseButton _button = MouseButton.None;

		public Task DownAsync(ClickOptions options = null)
		{
			options = options ?? new ClickOptions();

			var _button = options.Button;

			return SendDevtoolEvent("Input.dispatchMouseEvent", new InputDispatchMouseEventRequest
			{
				Type = MouseEventType.MousePressed,
				Button = "left",
				X = _x,
				Y = _y,
				Modifiers = 0,
				ClickCount = options.ClickCount
			});
		}

		public Task UpAsync(ClickOptions options = null)
		{
			options = options ?? new ClickOptions();

			_button = MouseButton.None;

			return SendDevtoolEvent("Input.dispatchMouseEvent", new InputDispatchMouseEventRequest
			{
				Type = MouseEventType.MouseReleased,
				Button = "left",
				X = _x,
				Y = _y,
				Modifiers = 0,
				ClickCount = options.ClickCount
			});
		}

		public async Task ClickAsync(decimal x, decimal y, ClickOptions options = null)
		{
			options = options ?? new ClickOptions();

			if (options.Delay > 0)
			{
				await Task.WhenAll(
					MoveAsync(x, y),
					DownAsync(options));

				await Task.Delay(options.Delay);
				await UpAsync();
			}
			else
			{
				await Task.WhenAll(
					DownAsync(options),
					UpAsync());
			}
		}

		private async void Form1_Load(object sender, EventArgs e)
		{
			// Loading cef set up
			await SetupCef();
		}
	}


	public class LowercaseContractResolver : DefaultContractResolver
	{
		protected override string ResolvePropertyName(string propertyName)
		{
			var f = propertyName.Substring(0, 1).ToLower();
			string newstr = f + propertyName.Remove(0, 1);
			return newstr;
		}
	}


	public enum EMouseKey
	{
		// Token: 0x0400005B RID: 91
		LEFT,

		// Token: 0x0400005C RID: 92
		RIGHT,

		// Token: 0x0400005D RID: 93
		DOUBLE_LEFT,

		// Token: 0x0400005E RID: 94
		DOUBLE_RIGHT
	}

	public class ClickOptions
	{
		/// <summary>
		/// Time to wait between <c>mousedown</c> and <c>mouseup</c> in milliseconds. Defaults to 0
		/// </summary>
		public int Delay { get; set; } = 0;

		/// <summary>
		/// Defaults to 1. See https://developer.mozilla.org/en-US/docs/Web/API/UIEvent/detail
		/// </summary>
		public int ClickCount { get; set; } = 1;

		/// <summary>
		/// The button to use for the click. Defaults to <see cref="MouseButton.Left"/>
		/// </summary>
		public MouseButton Button { get; set; } = MouseButton.Left;
	}

	public class MoveOptions
	{
		/// <summary>
		/// Sends intermediate <c>mousemove</c> events. Defaults to 1
		/// </summary>
		public int Steps { get; set; } = 1;
	}

	[JsonConverter(typeof(StringEnumConverter))]
	internal enum MouseEventType
	{
		[EnumMember(Value = "mouseMoved")]    MouseMoved,
		[EnumMember(Value = "mousePressed")]  MousePressed,
		[EnumMember(Value = "mouseReleased")] MouseReleased,
		[EnumMember(Value = "mouseWheel")]    MouseWheel
	}

	internal class InputDispatchMouseEventRequest
	{
		public MouseEventType Type { get; set; }

		public string Button { get; set; } = "left";

		public decimal X { get; set; }

		public decimal Y { get; set; }

		public int Modifiers { get; set; } = 0;

		public int ClickCount { get; set; }
	}
}