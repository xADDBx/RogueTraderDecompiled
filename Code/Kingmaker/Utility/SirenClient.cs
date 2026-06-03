using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Async;
using Core.Cheats;
using Kingmaker.Utility.Reporting.Base;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using UnityEngine.Networking;

namespace Kingmaker.Utility;

public class SirenClient
{
	private class ReportClient : IReportClient
	{
		private readonly LogChannel[] loggers;

		public ReportClient(LogChannel[] loggers)
		{
			this.loggers = loggers;
		}

		private static Uri GetUri(string address, NameValueCollection requestParameters)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = string.Empty;
			for (int i = 0; i < requestParameters.Count; i++)
			{
				stringBuilder.Append(text + requestParameters.AllKeys[i] + "=" + requestParameters[i]);
				text = "&";
			}
			return new UriBuilder(address)
			{
				Query = stringBuilder.ToString()
			}.Uri;
		}

		private static async Task<string> SendImplSwitch(string reportFileName, NameValueCollection parameters)
		{
			string address = "https://report.owlcat.games/report";
			Uri uri = GetUri(address, parameters);
			var (buffer, contentType) = InventSomeBytesSimilarToWebClient(reportFileName);
			await Awaiters.UnityThread;
			using UnityWebRequest request = new UnityWebRequest(uri, "POST", new DownloadHandlerBuffer(), new UploadHandlerRaw(buffer));
			request.SetRequestHeader("Content-Type", contentType);
			request.timeout = 1200;
			await request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.Success)
			{
				throw new Exception(request.error);
			}
			return request.downloadHandler.text;
		}

		private static (byte[] Buffer, string ContentType) InventSomeBytesSimilarToWebClient(string fileName)
		{
			using FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			string text = "---------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
			string item = "multipart/form-data; boundary=" + text;
			string s = "--" + text + "\r\nContent-Disposition: form-data; name=\"file\"; filename=\"" + Path.GetFileName(fileName) + "\"\r\nContent-Type: application/octet-stream\r\n\r\n";
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			byte[] bytes2 = Encoding.ASCII.GetBytes("\r\n--" + text + "--\r\n");
			byte[] array = new byte[fileStream.Length + bytes.Length + bytes2.Length];
			Array.Copy(bytes, array, bytes.Length);
			fileStream.Read(array, bytes.Length, (int)fileStream.Length);
			Array.Copy(bytes2, 0L, array, bytes.Length + fileStream.Length, bytes2.Length);
			return (Buffer: array, ContentType: item);
		}

		private static async Task<string> SendImplOthers(string reportFileName, NameValueCollection parameters)
		{
			using WebClientWithTimeout client = new WebClientWithTimeout(1200000);
			client.QueryString = parameters;
			string address = "https://report.owlcat.games/report";
			byte[] bytes = await client.UploadFileTaskAsync(address, "POST", reportFileName);
			return Encoding.UTF8.GetString(bytes);
		}

		public async Task<ReportUploadStatus> UploadAsync(string reportFileName, ReportSendParameters reportSendParameters, CancellationToken token)
		{
			NameValueCollection parameters = new NameValueCollection { 
			{
				"parameters",
				reportSendParameters.ToString()
			} };
			string text = ((!SendAsSwitch) ? (await SendImplOthers(reportFileName, parameters)) : (await SendImplSwitch(reportFileName, parameters)));
			try
			{
				return JsonConvert.DeserializeObject<ReportUploadStatus>(text);
			}
			catch (JsonReaderException)
			{
				LogChannel[] array = loggers;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Error("Server response isn't valid json. Response string: {0}", text);
				}
				return null;
			}
		}
	}

	private class TicketClient : ITicketClient
	{
		public async Task<FindTicketsResponse> FindTickets(FindTicketsRequest request)
		{
			using WebClientWithTimeout client = new WebClientWithTimeout(1200000);
			client.Headers[HttpRequestHeader.ContentType] = "application/json";
			ServicePointManager.ServerCertificateValidationCallback = (object _, X509Certificate _, X509Chain _, SslPolicyErrors _) => true;
			string data = JsonConvert.SerializeObject(request);
			string address = "https://siren.owlcat.local/api/tickets";
			return JsonConvert.DeserializeObject<FindTicketsResponse>(await client.UploadStringTaskAsync(address, data));
		}
	}

	public readonly IReportClient Report;

	public readonly ITicketClient Ticket;

	private readonly LogChannel[] loggers;

	[Cheat(Name = "send_reports_as_switch")]
	public static bool SendAsSwitch { get; set; }

	public SirenClient()
	{
		loggers = new LogChannel[2]
		{
			LogChannelFactory.GetOrCreate("SirenClient"),
			LogChannelFactory.GetOrCreate("Console")
		};
		Report = new ReportClient(loggers);
		Ticket = new TicketClient();
	}
}
