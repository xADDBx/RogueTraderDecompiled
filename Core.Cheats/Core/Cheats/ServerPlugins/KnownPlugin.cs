using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Core.RestServer.Common;
using Newtonsoft.Json;

namespace Core.Cheats.ServerPlugins;

public class KnownPlugin : IRestServerPlugin
{
	private static readonly JsonSerializer JsonSerializer = new JsonSerializer();

	private readonly CheatDatabase m_Database;

	public HttpMethod HttpMethod => HttpMethod.Get;

	public string LocalPath => "/Known";

	public KnownPlugin(CheatDatabase database)
	{
		m_Database = database;
	}

	public async Task Handle(HttpListenerContext context, CancellationToken cancellationToken)
	{
		HttpListenerResponse response = context.Response;
		string text = context.Request.Headers["If-None-Match"];
		response.ContentType = "application/json";
		if (text == m_Database.Version)
		{
			response.StatusCode = 304;
			return;
		}
		await using StreamWriter textWriter = new StreamWriter(response.OutputStream);
		using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
		response.Headers.Add(HttpResponseHeader.ETag, m_Database.Version);
		KnownObjectsInfo knownObjects = m_Database.GetKnownObjects();
		JsonSerializer.Serialize(jsonWriter, knownObjects);
	}
}
