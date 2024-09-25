using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Core.RestServer.Common;
using Newtonsoft.Json;

namespace Core.RestServer.Client;

public static class RestServerWatcher
{
	public class ProcessStatus
	{
		private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10.0);

		private RestServerStatus m_ServerStatus;

		public string Uri { get; }

		public int Port { get; }

		public DateTime LastSeen { get; private set; }

		public bool IsEditor => m_ServerStatus.IsEditor;

		public bool IsPlaying => m_ServerStatus.IsPlaying;

		public bool IsHeadless => m_ServerStatus.IsHeadless;

		public string LoadedMaps => string.Join(",", m_ServerStatus.LoadedMaps);

		public Guid Guid => m_ServerStatus.Guid;

		public bool IsExpired => DateTime.Now - LastSeen > Timeout;

		public ProcessStatus(string uri, int port, RestServerStatus status)
		{
			Uri = uri;
			Port = port;
			LastSeen = DateTime.Now;
			m_ServerStatus = status;
		}

		public void Update(RestServerStatus status)
		{
			LastSeen = DateTime.Now;
			m_ServerStatus = status;
		}

		public override string ToString()
		{
			string text = (IsExpired ? "(Expired!) " : "");
			string text2 = (IsEditor ? "Editor" : "Standalone");
			string text3 = (IsPlaying ? "Playing" : "Not Playing");
			return $"{text}{Uri}:{Port}; {text2}; {text3}; {LoadedMaps}";
		}
	}

	private const string LocalUri = "http://127.0.0.1";

	private static readonly JsonSerializer JsonSerializer = new JsonSerializer();

	private static readonly HttpClient HttpClient = new HttpClient();

	private static string[] s_WatchedUri = new string[0];

	private static readonly Dictionary<string, CancellationTokenSource[]> Watchers = new Dictionary<string, CancellationTokenSource[]>();

	private static readonly List<ProcessStatus> ProcessesList = new List<ProcessStatus>();

	public static IEnumerable<ProcessStatus> Processes
	{
		get
		{
			StartWatching("http://127.0.0.1");
			return ProcessesList;
		}
	}

	public static void StartWatching(string uri)
	{
		if (s_WatchedUri.Contains(uri))
		{
			return;
		}
		s_WatchedUri = s_WatchedUri.Append(uri).ToArray();
		Watchers[uri] = new CancellationTokenSource[100];
		foreach (int item in Enumerable.Range(35555, 100))
		{
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			Watch(uri, item, cancellationTokenSource.Token);
			Watchers[uri][item - 35555] = cancellationTokenSource;
		}
	}

	public static void StopWatching(string uri)
	{
		if (s_WatchedUri.Contains(uri))
		{
			s_WatchedUri = s_WatchedUri.Where((string i) => i != uri).ToArray();
			CancellationTokenSource[] array = Watchers[uri];
			for (int j = 0; j < array.Length; j++)
			{
				array[j].Cancel();
			}
			Watchers.Remove(uri);
		}
	}

	private static async void Watch(string uri, int port, CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			RestServerStatus? restServerStatus = await GetStatus(uri, port, cancellationToken);
			if (restServerStatus.HasValue)
			{
				ProcessStatus processStatus = ProcessesList.FirstOrDefault((ProcessStatus i) => i.Uri == uri && i.Port == port);
				if (processStatus != null)
				{
					processStatus.Update(restServerStatus.Value);
				}
				else
				{
					ProcessStatus item = new ProcessStatus(uri, port, restServerStatus.Value);
					ProcessesList.Add(item);
				}
			}
			await Task.Delay(1000, cancellationToken);
		}
	}

	private static async Task<RestServerStatus?> GetStatus(string uri, int port, CancellationToken ct)
	{
		_ = 2;
		try
		{
			string requestUri = $"{uri}:{port}/Status";
			RestServerStatus? result;
			await using (Stream stream = await (await HttpClient.GetAsync(requestUri, ct)).Content.ReadAsStreamAsync())
			{
				using StreamReader reader = new StreamReader(stream);
				using JsonTextReader reader2 = new JsonTextReader(reader);
				RestServerStatus value = JsonSerializer.Deserialize<RestServerStatus>(reader2);
				result = value;
			}
			return result;
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception)
		{
		}
		return null;
	}
}
