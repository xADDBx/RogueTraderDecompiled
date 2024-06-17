using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Core.RestServer.Common;
using Newtonsoft.Json;

namespace Core.StateCrawler.ServerPlugins;

public class DumpStatePlugin : IRestServerPlugin
{
	private static readonly JsonSerializer JsonSerializer = new JsonSerializer();

	public HttpMethod HttpMethod => HttpMethod.Post;

	public string LocalPath => "/DumpState";

	public async Task Handle(HttpListenerContext context, CancellationToken cancellationToken)
	{
		using StreamReader reader = new StreamReader(context.Request.InputStream);
		using JsonTextReader jsonReader = new JsonTextReader(reader);
		DumpStateInfo dumpStateInfo = JsonSerializer.Deserialize<DumpStateInfo>(jsonReader);
		context.Response.ContentType = "application/json";
		await using StreamWriter textWriter = new StreamWriter(context.Response.OutputStream);
		using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
		StateCrawler.Node state = StateCrawler.GetState(dumpStateInfo.RootObjectPath, dumpStateInfo.ExpandedChildren);
		JsonSerializer.Serialize(jsonWriter, state);
	}
}
