using UnityEngine;

namespace Core.Cheats;

internal static class ExampleCommands
{
	[Cheat]
	public static int TestVar { get; set; } = 1;


	[Cheat(ExecutionPolicy = ExecutionPolicy.EditMode)]
	public static int TestVarEditOnly { get; set; } = 1;


	[Cheat(ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static int TestVarPlayOnly { get; set; } = 1;


	[Cheat(Name = "test_command_editor_only", ExecutionPolicy = ExecutionPolicy.EditMode)]
	public static string TestCommandEditorOnly()
	{
		return "We are in " + (Application.isPlaying ? "play" : "edit") + " mode";
	}

	[Cheat(Name = "test_command_play_only", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static string TestCommandPlayOnly()
	{
		return "We are in " + (Application.isPlaying ? "play" : "edit") + " mode";
	}

	[Cheat(Name = "test_command")]
	public static string TestCommand()
	{
		return "We are in " + (Application.isPlaying ? "play" : "edit") + " mode";
	}

	[Cheat(Name = "test_command_with_default_arg")]
	public static string TestCommand(string arg = "Not set")
	{
		return "Arg is " + arg;
	}
}
