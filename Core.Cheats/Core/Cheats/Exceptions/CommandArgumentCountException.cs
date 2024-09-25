using System;

namespace Core.Cheats.Exceptions;

public class CommandArgumentCountException : Exception
{
	public readonly int MinArgs;

	public readonly int MaxArgs;

	public readonly int ActualArgs;

	public CommandArgumentCountException(int actualArgs, int minArgs, int maxArgs)
		: base($"Wrong count of arguments. Got {actualArgs}, Expected {minArgs}-{maxArgs}")
	{
		MinArgs = minArgs;
		MaxArgs = maxArgs;
		ActualArgs = actualArgs;
	}
}
