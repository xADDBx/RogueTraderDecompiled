using System;
using System.Collections.Generic;
using Core.Cheats;

namespace CheatsCodeGen;

public static class AllCheats
{
	public static readonly List<CheatMethodInfoInternal> Methods = new List<CheatMethodInfoInternal>
	{
		new CheatMethodInfoInternal(new Func<string, string, bool>(CheatBindings.Register), "bool Register(string binding, string command)", "bind", "", "", ExecutionPolicy.All, new CheatParameter[2]
		{
			new CheatParameter
			{
				Name = "binding",
				Type = "System.String",
				HasDefaultValue = false
			},
			new CheatParameter
			{
				Name = "command",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "bool"),
		new CheatMethodInfoInternal(new Func<string, bool>(CheatBindings.Unregister), "bool Unregister(string binding)", "unbind", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "binding",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "bool"),
		new CheatMethodInfoInternal(new Func<string>(CheatBindings.ListBindings), "string ListBindings()", "list_bindings", "", "", ExecutionPolicy.All, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Func<string>(ExampleCommands.TestCommandEditorOnly), "string TestCommandEditorOnly()", "test_command_editor_only", "", "", ExecutionPolicy.EditMode, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Func<string>(ExampleCommands.TestCommandPlayOnly), "string TestCommandPlayOnly()", "test_command_play_only", "", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Func<string>(ExampleCommands.TestCommand), "string TestCommand()", "test_command", "", "", ExecutionPolicy.All, new CheatParameter[0], "string"),
		new CheatMethodInfoInternal(new Func<string, string>(ExampleCommands.TestCommand), "string TestCommand(string arg = \\\"Not set\\\")", "test_command_with_default_arg", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "arg",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "string"),
		new CheatMethodInfoInternal(new Func<string, string>(HelpCommands.Help_New), "string Help_New(string cmdName = null)", "help_new", "", "", ExecutionPolicy.All, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "cmdName",
				Type = "System.String",
				HasDefaultValue = true
			}
		}, "string")
	};

	public static readonly List<CheatPropertyInfoInternal> Properties = new List<CheatPropertyInfoInternal>
	{
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<int>)(() => ExampleCommands.TestVar),
			Setter = (Action<int>)delegate(int value)
			{
				ExampleCommands.TestVar = value;
			}
		}, "TestVar", "", "", ExecutionPolicy.All, "int"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<int>)(() => ExampleCommands.TestVarEditOnly),
			Setter = (Action<int>)delegate(int value)
			{
				ExampleCommands.TestVarEditOnly = value;
			}
		}, "TestVarEditOnly", "", "", ExecutionPolicy.EditMode, "int"),
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<int>)(() => ExampleCommands.TestVarPlayOnly),
			Setter = (Action<int>)delegate(int value)
			{
				ExampleCommands.TestVarPlayOnly = value;
			}
		}, "TestVarPlayOnly", "", "", ExecutionPolicy.PlayMode, "int")
	};

	public static readonly List<(ArgumentConverter.ConvertDelegate, int)> ArgConverters = new List<(ArgumentConverter.ConvertDelegate, int)>
	{
		(ArgumentConverters.StringToEnumConverter, 0),
		(ArgumentConverters.StringToBoolConverter, 0),
		(ArgumentConverters.StringToGuidConverter, 0),
		(ArgumentConverters.NullConverter, -1),
		(ArgumentConverters.DefaultConverter, 1)
	};

	public static readonly List<(ArgumentConverter.PreprocessDelegate, int)> ArgPreprocessors = new List<(ArgumentConverter.PreprocessDelegate, int)>();
}
