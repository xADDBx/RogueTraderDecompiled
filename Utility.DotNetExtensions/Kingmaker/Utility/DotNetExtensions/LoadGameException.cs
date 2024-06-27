using System;
using System.Runtime.Serialization;

namespace Kingmaker.Utility.DotNetExtensions;

public class LoadGameException : Exception
{
	public LoadGameException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected LoadGameException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
