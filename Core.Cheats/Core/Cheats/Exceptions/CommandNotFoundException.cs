using System;

namespace Core.Cheats.Exceptions;

public class CommandNotFoundException : Exception
{
	public readonly string Command;

	public CommandNotFoundException(string command)
		: base("Command not found: " + command)
	{
		Command = command;
	}
}
