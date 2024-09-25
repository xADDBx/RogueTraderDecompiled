using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Core.RestServer.Common;
using Newtonsoft.Json;

namespace Core.Cheats.ServerPlugins;

public class BindingsPlugin : IRestServerPlugin
{
	private static readonly JsonSerializer JsonSerializer = new JsonSerializer();

	public HttpMethod HttpMethod => HttpMethod.Get;

	public string LocalPath => "/Bindings";

	public async Task Handle(HttpListenerContext context, CancellationToken cancellationToken)
	{
		HttpListenerResponse response = context.Response;
		string text = context.Request.Headers["If-None-Match"];
		response.ContentType = "application/json";
		if (text == CheatBindings.Version)
		{
			response.StatusCode = 304;
			response.Close();
			return;
		}
		await using StreamWriter textWriter = new StreamWriter(response.OutputStream);
		using JsonTextWriter jsonWriter = new JsonTextWriter(textWriter);
		response.Headers.Add(HttpResponseHeader.ETag, CheatBindings.Version);
		CheatBindingsInfo cheatBindingsInfo = default(CheatBindingsInfo);
		cheatBindingsInfo.Bindings = CheatBindings.ActiveBindings;
		CheatBindingsInfo cheatBindingsInfo2 = cheatBindingsInfo;
		JsonSerializer.Serialize(jsonWriter, cheatBindingsInfo2);
	}
}
