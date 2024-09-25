using System;

namespace Kingmaker.AI.DebugUtilities;

public class AILogExceptionMessage : AILogObject
{
	private readonly Exception exception;

	private readonly string message;

	public AILogExceptionMessage(Exception exception, string message)
	{
		this.exception = exception;
		this.message = message;
	}

	public override string GetLogString()
	{
		return $"{exception}\n{message}";
	}
}
