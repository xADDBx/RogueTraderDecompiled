using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Core.RestServer.Common;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;

namespace Core.Console.ServerPlugins;

public class ConsolePlugin : IRestServerPlugin
{
	private static readonly JsonSerializer JsonSerializer = new JsonSerializer();

	private static readonly LogEntry[] NullEntries = new LogEntry[1]
	{
		new LogEntry(0, DateTime.MinValue, LogSeverity.Message, "", "No log delegate", string.Empty)
	};

	private readonly Func<Guid, LogEntry[]> m_LogsGetter;

	public HttpMethod HttpMethod => HttpMethod.Post;

	public string LocalPath => "/Log";

	public ConsolePlugin(Func<Guid, LogEntry[]> logsGetter)
	{
		m_LogsGetter = logsGetter;
	}

	public async Task Handle(HttpListenerContext context, CancellationToken cancellationToken)
	{
		using StreamReader reader = new StreamReader(context.Request.InputStream);
		Guid arg = Guid.Parse(await reader.ReadToEndAsync());
		context.Response.ContentType = "application/json";
		await using StreamWriter textWriter = new StreamWriter(context.Response.OutputStream);
		using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
		if (m_LogsGetter == null)
		{
			LogEntry[] nullEntries = NullEntries;
			JsonSerializer.Serialize(jsonWriter, nullEntries);
		}
		else
		{
			LogEntry[] value = m_LogsGetter(arg);
			JsonSerializer.Serialize(jsonWriter, value);
		}
	}
}
