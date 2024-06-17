using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Core.StateCrawler;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Core.RestServer.Client;

public sealed class RestServerClient
{
	private readonly JsonSerializer m_JsonSerializer = new JsonSerializer();

	private readonly HttpClient m_HttpClient = new HttpClient();

	private readonly string m_Uri;

	private readonly int m_Port;

	[CanBeNull]
	public RestServerWatcher.ProcessStatus ProcessStatus => RestServerWatcher.Processes.FirstOrDefault((RestServerWatcher.ProcessStatus i) => i.Uri == m_Uri && i.Port == m_Port);

	public RestServerClient(string uri, int port)
	{
		m_Uri = uri;
		m_Port = port;
	}

	public async Task<Core.StateCrawler.StateCrawler.Node?> DumpState(DumpStateInfo info, CancellationToken cancellationToken)
	{
		_ = 2;
		try
		{
			string requestUri = $"{m_Uri}:{m_Port}/DumpState";
			using (MemoryStream stream = new MemoryStream())
			{
				using JsonTextWriter jsonWriter = new JsonTextWriter(new StreamWriter(stream));
				m_JsonSerializer.Serialize(jsonWriter, info);
				jsonWriter.Flush();
				stream.Seek(0L, SeekOrigin.Begin);
				using StreamContent payload = new StreamContent(stream);
				using HttpResponseMessage response = await m_HttpClient.PostAsync(requestUri, payload, cancellationToken);
				Core.StateCrawler.StateCrawler.Node? result;
				await using (Stream stream2 = await response.Content.ReadAsStreamAsync())
				{
					response.EnsureSuccessStatusCode();
					using JsonTextReader reader = new JsonTextReader(new StreamReader(stream2));
					Core.StateCrawler.StateCrawler.Node value = m_JsonSerializer.Deserialize<Core.StateCrawler.StateCrawler.Node>(reader);
					result = value;
				}
				return result;
			}
			IL_0328:
			Core.StateCrawler.StateCrawler.Node? result2;
			return result2;
		}
		catch (Exception ex)
		{
			Core.StateCrawler.StateCrawler.Node value2 = default(Core.StateCrawler.StateCrawler.Node);
			value2.Name = info.RootObjectPath;
			value2.Value = ex.Message + "\n" + ex.StackTrace;
			return value2;
		}
	}
}
