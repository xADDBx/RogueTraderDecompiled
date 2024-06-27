using System;
using System.Runtime.Serialization;

namespace Kingmaker.Utility.DotNetExtensions;

public class LoadingInterruptedByUserException : Exception
{
	public LoadingInterruptedByUserException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected LoadingInterruptedByUserException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
