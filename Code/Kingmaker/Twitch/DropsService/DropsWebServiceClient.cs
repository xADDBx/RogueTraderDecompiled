using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kingmaker.Twitch.DropsService.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Twitch.DropsService;

public class DropsWebServiceClient : IDisposable
{
	public class Response<TBody>
	{
		private readonly HttpResponseMessage m_ResponseMessage;

		public bool IsSuccessStatusCode => m_ResponseMessage.IsSuccessStatusCode;

		public HttpStatusCode StatusCode => m_ResponseMessage.StatusCode;

		public Response(HttpResponseMessage responseMessage)
		{
			m_ResponseMessage = responseMessage;
		}

		public async Task<TBody> GetBodyAsync()
		{
			string text = await m_ResponseMessage.Content.ReadAsStringAsync();
			Log.Log("Response Body: " + text);
			return JsonConvert.DeserializeObject<TBody>(text, new JsonConverter[1] { EnumConverter });
		}
	}

	private readonly string m_BaseUrl;

	private readonly HttpClient m_HttpClient = new HttpClient();

	private static readonly LogChannel Log = LogChannelFactory.GetOrCreate("DropsWebServiceClient");

	private static readonly StringEnumConverter EnumConverter = new StringEnumConverter();

	private CancellationTokenSource m_GetDropsCts;

	public DropsWebServiceClient(string baseUrl, float timeoutSeconds = 30f)
	{
		m_BaseUrl = baseUrl;
		m_HttpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
	}

	public void Dispose()
	{
		m_HttpClient.CancelPendingRequests();
		m_HttpClient.Dispose();
		m_GetDropsCts?.Cancel();
		m_GetDropsCts?.Dispose();
	}

	public Task<Response<DropsResponseBody>> GetDropsAsync(string gameUid, IReadOnlyCollection<string> rewardIds, bool exclude = false)
	{
		m_GetDropsCts?.Cancel();
		m_GetDropsCts?.Dispose();
		m_GetDropsCts = new CancellationTokenSource();
		HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, BuildGetDropsRequestUri(gameUid, rewardIds, exclude));
		return SendRequestAsync<DropsResponseBody>(httpRequest, m_GetDropsCts.Token);
	}

	public Task<Response<ClaimDropsResponseBody>> ClaimDropsAsync(string gameUid, IReadOnlyCollection<string> rewardIds)
	{
		HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, BuildClaimDropsRequestUri(gameUid));
		httpRequestMessage.Content = BuildClaimDropsRequestBody(rewardIds);
		return SendRequestAsync<ClaimDropsResponseBody>(httpRequestMessage, CancellationToken.None);
	}

	public Uri GetLinkPageUri(string gameUid)
	{
		return new Uri(m_BaseUrl + "/link/" + Uri.EscapeDataString(gameUid), UriKind.Absolute);
	}

	private Uri BuildGetDropsRequestUri(string gameUid, IReadOnlyCollection<string> rewardIds, bool exclude)
	{
		string text2;
		if (rewardIds != null && rewardIds.Count > 0)
		{
			string text = string.Join("&", rewardIds.Select((string id) => "rewards[]=" + Uri.EscapeDataString(id)));
			text2 = "?exclude=" + (exclude ? "true" : "false") + "&" + text;
		}
		else
		{
			text2 = "";
		}
		return new Uri(m_BaseUrl + "/api/drops/" + Uri.EscapeDataString(gameUid) + text2, UriKind.Absolute);
	}

	private Uri BuildClaimDropsRequestUri(string gameUid)
	{
		return new Uri(m_BaseUrl + "/api/drops/claim/" + Uri.EscapeDataString(gameUid), UriKind.Absolute);
	}

	private static HttpContent BuildClaimDropsRequestBody(IEnumerable<string> rewardIds)
	{
		string text = JsonConvert.SerializeObject(new ClaimDropsRequestBody
		{
			Rewards = rewardIds.ToList()
		});
		Log.Log("Request body: " + text);
		return new StringContent(text, Encoding.UTF8, "application/json");
	}

	private async Task<Response<TBody>> SendRequestAsync<TBody>(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
	{
		try
		{
			Log.Log($"{httpRequest.Method} {httpRequest.RequestUri} ...");
			HttpResponseMessage httpResponseMessage = await m_HttpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseContentRead, cancellationToken);
			Log.Log($"Response {(int)httpResponseMessage.StatusCode}, Content Length: {httpResponseMessage.Content.Headers.ContentLength}, Type: {httpResponseMessage.Content.Headers.ContentType}");
			return new Response<TBody>(httpResponseMessage);
		}
		catch (HttpRequestException ex)
		{
			Log.Exception(ex);
		}
		catch (TaskCanceledException ex2)
		{
			Log.Exception(ex2);
		}
		return null;
	}
}
