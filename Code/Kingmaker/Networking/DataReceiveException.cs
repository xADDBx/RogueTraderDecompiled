using System;

namespace Kingmaker.Networking;

public class DataReceiveException : Exception
{
	public DataReceiveException(string message)
		: base(message)
	{
	}

	public DataReceiveException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
