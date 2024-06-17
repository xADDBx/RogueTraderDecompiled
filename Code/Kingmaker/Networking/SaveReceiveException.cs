using System;

namespace Kingmaker.Networking;

public class SaveReceiveException : Exception
{
	public SaveReceiveException(string message)
		: base(message)
	{
	}

	public SaveReceiveException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
