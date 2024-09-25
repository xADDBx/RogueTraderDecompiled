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

public class CommandPlugin : IRestServerPlugin
{
	private static readonly JsonSerializer JsonSerializer = new JsonSerializer();

	private readonly CheatsParser.ExecuteCommandDelegate m_Executor;

	public HttpMethod HttpMethod => HttpMethod.Post;

	public string LocalPath => "/Command";

	public CommandPlugin(CheatsParser.ExecuteCommandDelegate executor)
	{
		m_Executor = executor;
	}

	public async Task Handle(HttpListenerContext context, CancellationToken cancellationToken)
	{
		using StreamReader reader = new StreamReader(context.Request.InputStream);
		using JsonTextReader jsonReader = new JsonTextReader(reader);
		ExecuteCommandInfo executeCommandInfo = JsonSerializer.Deserialize<ExecuteCommandInfo>(jsonReader);
		context.Response.ContentType = "text/plain";
		await using StreamWriter stream = new StreamWriter(context.Response.OutputStream);
		using JsonTextWriter writer = new JsonTextWriter(stream);
		try
		{
			await m_Executor(executeCommandInfo.CommandName, executeCommandInfo.Args);
		}
		catch (OperationCanceledException)
		{
		}
		catch (CommandNotFoundException ex2)
		{
			context.Response.StatusCode = 404;
			JsonSerializer.Serialize(writer, ex2.ToString());
		}
		catch (Exception ex3)
		{
			if (ex3 is CommandParseException || ex3 is CommandArgumentCountException || ex3 is ArgumentException)
			{
				context.Response.StatusCode = 400;
				JsonSerializer.Serialize(writer, ex3.ToString());
			}
			else
			{
				context.Response.StatusCode = 500;
				JsonSerializer.Serialize(writer, ex3.ToString());
			}
		}
	}
}
