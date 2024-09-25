using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Core.Async;
using Core.RestServer.Common;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Core.RestServer;

public class RestServer : IDisposable
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("RestServer");

	private static readonly JsonSerializer JsonSerializer = new JsonSerializer();

	private readonly HttpListener m_Listener = new HttpListener();

	private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

	private readonly Guid m_Guid = Guid.NewGuid();

	private readonly IRestServerPlugin[] m_Plugins;

	public int Port { get; set; }

	public RestServer(IRestServerPlugin[] plugins)
	{
		m_Plugins = plugins;
		IGrouping<string, IRestServerPlugin>[] array = (from i in m_Plugins
			group i by i.HttpMethod?.ToString() + i.LocalPath into i
			where i.Count() > 1
			select i).ToArray();
		foreach (IGrouping<string, IRestServerPlugin> source in array)
		{
			IRestServerPlugin restServerPlugin = source.First();
			Logger.Log("Conflicting plugins for REST server: {0} {1} {2}", restServerPlugin.HttpMethod, restServerPlugin.LocalPath, string.Join(", ", source.Select((IRestServerPlugin p) => p.GetType().Name)));
		}
	}

	public Task Start()
	{
		CancellationToken ct = m_CancellationTokenSource.Token;
		return Task.Run(delegate
		{
			StartImpl(ct);
		}, ct);
	}

	private void StartImpl(CancellationToken ct)
	{
		int num = 0;
		while (true)
		{
			ct.ThrowIfCancellationRequested();
			int num2 = 35555 + num++;
			try
			{
				m_Listener.Prefixes.Clear();
				m_Listener.Prefixes.Add($"http://*:{num2}/");
				m_Listener.Start();
				Task.Run(() => Listen(ct), ct);
				Logger.Log("Initialized REST server on port {0}", num2);
				break;
			}
			catch (Exception innerException)
			{
				if (num <= 100)
				{
					continue;
				}
				throw new Exception("Failed to initialize REST server", innerException);
			}
		}
	}

	public void Dispose()
	{
		m_CancellationTokenSource.Cancel();
		m_Listener.Close();
	}

	private async Task Listen(CancellationToken ct)
	{
		while (true)
		{
			try
			{
				ProcessRequest(await m_Listener.GetContextAsync(), ct);
			}
			catch (Exception ex)
			{
				if (!ct.IsCancellationRequested)
				{
					Logger.Exception(ex);
				}
				break;
			}
		}
	}

	private async void ProcessRequest(HttpListenerContext context, CancellationToken ct)
	{
		try
		{
			try
			{
				if (context.Request.HttpMethod == "GET" && context.Request.Url.LocalPath == "/Status")
				{
					await Awaiters.UnityThread;
					Status(context.Response);
					goto end_IL_0057;
				}
				IRestServerPlugin[] plugins = m_Plugins;
				int num = 0;
				while (true)
				{
					if (num < plugins.Length)
					{
						IRestServerPlugin plugin = plugins[num];
						if (!(context.Request.HttpMethod == plugin.HttpMethod.Method) || !(context.Request.Url.LocalPath == plugin.LocalPath))
						{
							num++;
							continue;
						}
						await Awaiters.UnityThread;
						await plugin.Handle(context, ct);
						return;
					}
					context.Response.StatusCode = 404;
					context.Response.ContentType = "text/plain";
					await using (StreamWriter streamWriter = new StreamWriter(context.Response.OutputStream))
					{
						await streamWriter.WriteAsync("Not found");
					}
					break;
				}
				goto end_IL_0035;
				end_IL_0057:;
			}
			catch (Exception e)
			{
				if (ct.IsCancellationRequested)
				{
					goto end_IL_03ec;
				}
				try
				{
					context.Response.StatusCode = 500;
					context.Response.ContentType = "text/plain";
					await using StreamWriter streamWriter = new StreamWriter(context.Response.OutputStream);
					await streamWriter.WriteAsync(e.ToString());
				}
				catch (ObjectDisposedException)
				{
				}
				catch (Exception ex2)
				{
					Logger.Exception(e);
					Logger.Exception(ex2);
				}
				goto end_IL_0035;
				end_IL_03ec:;
			}
			end_IL_0035:;
		}
		finally
		{
			await context.Response.OutputStream.DisposeAsync();
		}
	}

	private static string[] GetMaps()
	{
		return (from v in Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt)
			where v.isLoaded
			select v.name).ToArray();
	}

	private RestServerStatus GetStatus()
	{
		RestServerStatus result = default(RestServerStatus);
		result.Guid = m_Guid;
		result.LoadedMaps = GetMaps();
		result.IsEditor = Application.isEditor;
		result.IsPlaying = Application.isPlaying;
		result.IsHeadless = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
		result.ProcessID = Process.GetCurrentProcess().Id;
		return result;
	}

	private void Status(HttpListenerResponse resp)
	{
		resp.ContentType = "application/json";
		using StreamWriter textWriter = new StreamWriter(resp.OutputStream);
		using JsonWriter jsonWriter = new JsonTextWriter(textWriter);
		RestServerStatus status = GetStatus();
		JsonSerializer.Serialize(jsonWriter, status);
	}
}
