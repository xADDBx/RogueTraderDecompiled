using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Networking.Hash;
using Kingmaker.Networking.Serialization;
using Kingmaker.Settings;
using Kingmaker.Utility.Serialization;
using Newtonsoft.Json;

namespace Kingmaker.Networking.Desync;

public class SendToRemoteDesyncHandler : IDesyncHandler
{
	private readonly struct RemoteDesyncMeta
	{
		public readonly int Tick;

		public readonly string Rid;

		public readonly int PlayersCount;

		public readonly Dictionary<string, string> Props;

		public RemoteDesyncMeta(DesyncMeta clientMeta, Dictionary<string, string> props)
		{
			Tick = clientMeta.Tick;
			Rid = clientMeta.RoomId;
			PlayersCount = clientMeta.PlayersCount;
			Props = props;
		}
	}

	private const string RemoteEndpoint = "http://89.17.52.236:5060/api/desync/upload";

	private static readonly HttpClient Client = new HttpClient();

	private readonly IPropsCollector m_PropsCollector;

	public SendToRemoteDesyncHandler(IPropsCollector propsCollector)
	{
		m_PropsCollector = propsCollector;
	}

	public async void RaiseDesync(HashableState state, DesyncMeta meta)
	{
		if (!SettingsRoot.Game.Main.SendGameStatistic)
		{
			PFLog.Net.Log("[Desync] SendGameStatistic is false. Don't send anything.");
		}
		else
		{
			await SendToElasticProxySafe(state, meta);
		}
	}

	private async ValueTask SendToElasticProxySafe(HashableState state, DesyncMeta meta)
	{
		try
		{
			await SendToElasticProxy(state, meta);
		}
		catch (Exception ex)
		{
			PFLog.Net.Exception(ex);
		}
	}

	private async ValueTask SendToElasticProxy(HashableState state, DesyncMeta meta)
	{
		string state2 = GameStateJsonSerializer.Serializer.SerializeObject(state);
		var (httpStatusCode, text) = await SendToElasticProxy(state2, meta);
		PFLog.Net.Error($"[Desync] Desynced state uploaded ({meta.Tick},{meta.RoomId},{meta.PlayersCount}) with response status code: {httpStatusCode} ({text}).");
	}

	public async ValueTask<(HttpStatusCode, string)> SendToElasticProxy(string state, DesyncMeta meta)
	{
		MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
		string content = JsonConvert.SerializeObject(new RemoteDesyncMeta(meta, m_PropsCollector.Collect()));
		multipartFormDataContent.Add(new StringContent(content, Encoding.UTF8, "application/json"), "files", "meta.json");
		multipartFormDataContent.Add(new StringContent(state, Encoding.UTF8, "application/json"), "files", "state.json");
		using HttpResponseMessage response = await Client.PostAsync("http://89.17.52.236:5060/api/desync/upload", multipartFormDataContent);
		HttpStatusCode statusCode = response.StatusCode;
		return (statusCode, await response.Content.ReadAsStringAsync());
	}
}
