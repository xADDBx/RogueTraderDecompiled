using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kingmaker.QA.Arbiter;

public class ArbiterStatusServer : IDisposable
{
	private const int DefaultPort = 44444;

	private static readonly JsonSerializer _jsonSerializer = new JsonSerializer();

	private readonly HttpListener _listener = new HttpListener();

	private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

	public DateTime LastStatus { get; set; } = DateTime.Now;


	public ArbiterStatusServer()
	{
		try
		{
			int num = SelectPort();
			string text = $"http://*:{num}/";
			_listener.Prefixes.Clear();
			_listener.Prefixes.Add(text);
			_listener.Start();
			Listen(_cancellationTokenSource.Token);
			PFLog.Arbiter.Log("ArbiterStatusServer started at " + text);
		}
		catch (Exception arg)
		{
			PFLog.Arbiter.Error("Failed to start ArbiterStatusServer");
			PFLog.Arbiter.Error($"ArbiterStatusServer error: {arg}");
		}
	}

	public void Dispose()
	{
		if (_listener.IsListening)
		{
			_cancellationTokenSource.Cancel();
			_listener.Close();
		}
	}

	private static int SelectPort()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			string[] array = commandLineArgs[i].Trim().Split('=');
			dictionary.Add(array[0], (array.Length > 1) ? array[1] : string.Empty);
		}
		if (!dictionary.ContainsKey("-ArbiterStatusPort"))
		{
			return 44444;
		}
		if (!int.TryParse(dictionary["-ArbiterStatusPort"], out var result))
		{
			return 44444;
		}
		return result;
	}

	private async Task Listen(CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				ProcessRequest(await _listener.GetContextAsync());
			}
			catch (Exception arg)
			{
				PFLog.Arbiter.Error($"ArbiterStatusServer error: {arg}");
			}
		}
	}

	private void ProcessRequest(HttpListenerContext context)
	{
		using (context.Response)
		{
			try
			{
				using StreamReader reader = new StreamReader(context.Request.InputStream);
				using (new JsonTextReader(reader))
				{
					if (context.Request.HttpMethod == "GET" && context.Request.Url.LocalPath == "/Status")
					{
						LastStatus = DateTime.Now;
						Status(context.Response);
						return;
					}
					context.Response.StatusCode = 404;
					context.Response.ContentType = "text/plain";
					using StreamWriter streamWriter = new StreamWriter(context.Response.OutputStream);
					streamWriter.Write("Not found");
				}
			}
			catch (Exception ex)
			{
				try
				{
					context.Response.StatusCode = 500;
					context.Response.ContentType = "text/plain";
					using StreamWriter streamWriter2 = new StreamWriter(context.Response.OutputStream);
					streamWriter2.Write(ex.ToString());
				}
				catch (Exception)
				{
				}
			}
		}
	}

	private void Status(HttpListenerResponse resp)
	{
		resp.ContentType = "application/json";
		using StreamWriter textWriter = new StreamWriter(resp.OutputStream);
		using JsonWriter jsonWriter = new JsonTextWriter(textWriter);
		string status = Arbiter.GetStatus();
		PFLog.Arbiter.Log("Status response: " + status);
		_jsonSerializer.Serialize(jsonWriter, status);
	}
}
