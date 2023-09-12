﻿using CefSharp.Callback;
using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudfare_bypass_cefsharp
{
	public class TaskMethodDevToolsMessageObserver : IDevToolsMessageObserver
	{
		private TaskCompletionSource<Tuple<bool, byte[]>> taskCompletionSource { get; set; } =
			new TaskCompletionSource<Tuple<bool, byte[]>>();

		private readonly int matchMessageId;

		public TaskMethodDevToolsMessageObserver(int messageId)
		{
			matchMessageId = messageId;
		}

		void IDisposable.Dispose()
		{
		}

		void IDevToolsMessageObserver.OnDevToolsAgentAttached(IBrowser browser)
		{
		}

		void IDevToolsMessageObserver.OnDevToolsAgentDetached(IBrowser browser)
		{
		}

		void IDevToolsMessageObserver.OnDevToolsEvent(IBrowser browser, string method, Stream parameters)
		{
		}

		bool IDevToolsMessageObserver.OnDevToolsMessage(IBrowser browser, Stream message)
		{
			return false;
		}

		void IDevToolsMessageObserver.OnDevToolsMethodResult(IBrowser browser, int messageId, bool success,
															 Stream result)
		{
			//We found the message Id we're after
			if (matchMessageId == messageId)
			{
				var memoryStream = new MemoryStream((int)result.Length);

				result.CopyTo(memoryStream);

				var response = Tuple.Create(success, memoryStream.ToArray());

				taskCompletionSource.TrySetResult(response);
			}
		}

		public Task<Tuple<bool, byte[]>> Task => taskCompletionSource.Task;
	}
}