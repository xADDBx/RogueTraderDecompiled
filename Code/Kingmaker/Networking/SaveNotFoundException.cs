using System;

namespace Kingmaker.Networking;

public class SaveNotFoundException : Exception
{
	public SaveNotFoundException(string message)
		: base(message)
	{
	}
}
