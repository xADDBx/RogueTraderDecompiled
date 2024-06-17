using System.Collections.Specialized;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kingmaker.Utility.Reporting.Base;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;

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

		public async Task<ReportUploadStatus> UploadAsync(string reportFileName, ReportSendParameters reportSendParameters, CancellationToken token)
		{
			using WebClientWithTimeout client = new WebClientWithTimeout(1200000);
			NameValueCollection queryString = new NameValueCollection { 
			{
				"parameters",
				reportSendParameters.ToString()
			} };
			client.QueryString = queryString;
			string address = await SirenAddressFinder.GetReportServerAddressAsync(token) + "/report";
			byte[] bytes = await client.UploadFileTaskAsync(address, "POST", reportFileName);
			string @string = Encoding.UTF8.GetString(bytes);
			try
			{
				return JsonConvert.DeserializeObject<ReportUploadStatus>(@string);
			}
			catch (JsonReaderException)
			{
				LogChannel[] array = loggers;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Error("Server response isn't valid json. Response string: {0}", @string);
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
			string sirenAddress = SirenAddressFinder.GetSirenAddress();
			sirenAddress += "/api/tickets";
			return JsonConvert.DeserializeObject<FindTicketsResponse>(await client.UploadStringTaskAsync(sirenAddress, data));
		}
	}

	public readonly IReportClient Report;

	public readonly ITicketClient Ticket;

	private readonly LogChannel[] loggers;

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
