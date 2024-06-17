using System;

namespace Core.Cheats.Exceptions;

public class ExecutionPolicyException : Exception
{
	public readonly string CmdName;

	public readonly ExecutionPolicy ExecutionPolicy;

	public readonly bool IsPlaying;

	public ExecutionPolicyException(string cmdName, ExecutionPolicy executionPolicy, bool isPlaying)
		: base(string.Format("Cant execute {0} registered with policy {1} while in {2} mode", cmdName, executionPolicy, isPlaying ? "play" : "edit"))
	{
		CmdName = cmdName;
		ExecutionPolicy = executionPolicy;
		IsPlaying = isPlaying;
	}
}
