using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Core.RestServer.Common;
using Newtonsoft.Json;

namespace Core.Cheats.ServerPlugins;

public class AutoCompletePlugin : IRestServerPlugin
{
	private static readonly JsonSerializer JsonSerializer = new JsonSerializer();

	private readonly CheatsParser m_Parser;

	public HttpMethod HttpMethod => HttpMethod.Post;

	public string LocalPath => "/AutoComplete";

	public AutoCompletePlugin(CheatsParser cheatsParser)
	{
		m_Parser = cheatsParser;
	}

	public async Task Handle(HttpListenerContext context, CancellationToken cancellationToken)
	{
		using StreamReader reader = new StreamReader(context.Request.InputStream);
		string pieceOfCommand = await reader.ReadToEndAsync();
		context.Response.ContentType = "application/json";
		await using StreamWriter textWriter = new StreamWriter(context.Response.OutputStream);
		using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
		string[] value = m_Parser.TryAutocomplete(pieceOfCommand).ToArray();
		JsonSerializer.Serialize(jsonWriter, value);
	}
}
