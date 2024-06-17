using System;

namespace Kingmaker.Networking.Exceptions;

public class NetworkConnectionException : Exception
{
	public NetworkConnectionException(string message)
		: base(message)
	{
	}
}
