using System;
using System.Net;

namespace Kingmaker.Utility;

public class WebClientWithTimeout : WebClient
{
	public readonly int Timeout;

	public WebClientWithTimeout(int timeout)
	{
		Timeout = timeout;
	}

	protected override WebRequest GetWebRequest(Uri address)
	{
		WebRequest webRequest = base.GetWebRequest(address);
		if (webRequest != null)
		{
			webRequest.Timeout = Timeout;
		}
		return webRequest;
	}
}
