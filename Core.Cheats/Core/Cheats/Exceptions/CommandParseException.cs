using System;

namespace Core.Cheats.Exceptions;

public class CommandParseException : Exception
{
	public readonly string Input;

	public CommandParseException(string input)
		: base(string.IsNullOrWhiteSpace(input) ? "Command is empty" : ("Failed to parse " + input + " as command"))
	{
		Input = input;
	}
}
