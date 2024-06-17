using System;

namespace Kingmaker.Networking;

public class PhotonCustomAuthenticationFailedException : Exception
{
	public PhotonCustomAuthenticationFailedException(string debugMessage)
		: base(debugMessage)
	{
	}
}
