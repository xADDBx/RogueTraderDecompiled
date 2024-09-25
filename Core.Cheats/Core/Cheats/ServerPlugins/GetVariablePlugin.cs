using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Core.Cheats.Exceptions;
using Core.RestServer.Common;
using Newtonsoft.Json;

namespace Core.Cheats.ServerPlugins;

public class GetVariablePlugin : IRestServerPlugin
{
	private static readonly JsonSerializer JsonSerializer = new JsonSerializer();

	private readonly CheatsParser.ExecuteGetVariableDelegate m_Executor;

	public HttpMethod HttpMethod => HttpMethod.Post;

	public string LocalPath => "/GetVariable";

	public GetVariablePlugin(CheatsParser.ExecuteGetVariableDelegate executor)
	{
		m_Executor = executor;
	}

	public async Task Handle(HttpListenerContext context, CancellationToken cancellationToken)
	{
		using StreamReader reader = new StreamReader(context.Request.InputStream);
		using JsonTextReader jsonReader = new JsonTextReader(reader);
		GetVariableInfo getVariableInfo = JsonSerializer.Deserialize<GetVariableInfo>(jsonReader);
		context.Response.ContentType = "text/plain";
		await using StreamWriter stream = new StreamWriter(context.Response.OutputStream);
		using JsonTextWriter writer = new JsonTextWriter(stream);
		try
		{
			await m_Executor(getVariableInfo.VariableName);
		}
		catch (CommandNotFoundException ex)
		{
			context.Response.StatusCode = 404;
			JsonSerializer.Serialize(writer, ex.ToString());
		}
		catch (Exception ex2)
		{
			if (ex2 is CommandParseException || ex2 is CommandArgumentCountException || ex2 is ArgumentException)
			{
				context.Response.StatusCode = 400;
				JsonSerializer.Serialize(writer, ex2.ToString());
			}
			else
			{
				context.Response.StatusCode = 500;
				JsonSerializer.Serialize(writer, ex2.ToString());
			}
		}
	}
}
