using System;

namespace Kingmaker.Networking;

public class SendMessageFailException : Exception
{
	public SendMessageFailException(string message)
		: base(message)
	{
	}
}
