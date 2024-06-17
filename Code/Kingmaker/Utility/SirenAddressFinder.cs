using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Utility;

public static class SirenAddressFinder
{
	private static LogChannel Logger = LogChannelFactory.GetOrCreate("SirenAddressFinder");

	private static string s_ApprovedAddress;

	public static string GetSirenAddress()
	{
		return "http://siren.owlcat.local";
	}

	public static async ValueTask<string> GetReportServerAddressAsync(CancellationToken token)
	{
		if (s_ApprovedAddress != null)
		{
			Logger.Log("Use siren server on '" + s_ApprovedAddress + "'");
			return s_ApprovedAddress;
		}
		ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
		ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, (RemoteCertificateValidationCallback)((object _, X509Certificate _, X509Chain _, SslPolicyErrors _) => true));
		string[] array = new string[7]
		{
			BuildModeUtility.Address,
			"http://siren.owlcat.local:5050",
			"http://siren-downloader.owlcat.local:5050",
			"https://report.owlcat.games",
			"http://46.199.74.205:5050/",
			"http://81.4.182.101:5050",
			"http://89.17.52.236:5050"
		};
		string[] array2 = array;
		foreach (string address in array2)
		{
			token.ThrowIfCancellationRequested();
			if (string.IsNullOrWhiteSpace(address))
			{
				continue;
			}
			try
			{
				using HttpClient client = new HttpClient();
				if (!(await client.GetAsync(address, token)).IsSuccessStatusCode)
				{
					continue;
				}
				Logger.Log("Found siren server on '{0}'", address);
				return s_ApprovedAddress = address;
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "Siren server on '{0}' unreachable", address);
			}
		}
		throw new InvalidOperationException("Cannot find Siren");
	}
}
