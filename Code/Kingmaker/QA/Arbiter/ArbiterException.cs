using System;

namespace Kingmaker.QA.Arbiter;

public class ArbiterException : Exception
{
	public ArbiterException(string message)
		: base(message)
	{
	}
}
