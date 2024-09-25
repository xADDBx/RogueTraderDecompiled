using System;

namespace Kingmaker.Networking;

public class PlayerCommandInsideEffectContextException : Exception
{
	public PlayerCommandInsideEffectContextException(string message)
		: base(message)
	{
	}
}
