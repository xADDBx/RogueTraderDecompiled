using System;

namespace Kingmaker.Networking;

public class GetAuthDataException : Exception
{
	public readonly int ErrorCode;

	public GetAuthDataException(int errorCode)
	{
		ErrorCode = errorCode;
	}

	public GetAuthDataException(int errorCode, string message)
		: base(message)
	{
		ErrorCode = errorCode;
	}
}
