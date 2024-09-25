using System;
using System.Collections.Generic;
using Core.Cheats;
using Owlcat.Core.Overlays;

namespace CheatsCodeGen;

public static class AllCheats
{
	public static readonly List<CheatMethodInfoInternal> Methods = new List<CheatMethodInfoInternal>
	{
		new CheatMethodInfoInternal(new Action<string>(Commands.Toggle), "void Toggle(string name)", "ovr_toggle", "Toggle visibility of performance overlay by name", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "name",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void"),
		new CheatMethodInfoInternal(new Action(Commands.List), "void List()", "ovr_list", "List all registered overlays", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action(Commands.ListGraphs), "void ListGraphs()", "ovr_list_graphs", "List all graphs in the active overlay", "", ExecutionPolicy.PlayMode, new CheatParameter[0], "void"),
		new CheatMethodInfoInternal(new Action<string>(Commands.ToggleGraph), "void ToggleGraph(string name)", "ovr_toggle_graph", "Toggle visibility of a graph in currently visible by name", "", ExecutionPolicy.PlayMode, new CheatParameter[1]
		{
			new CheatParameter
			{
				Name = "name",
				Type = "System.String",
				HasDefaultValue = false
			}
		}, "void")
	};

	public static readonly List<CheatPropertyInfoInternal> Properties = new List<CheatPropertyInfoInternal>
	{
		new CheatPropertyInfoInternal(new CheatPropertyMethods
		{
			Getter = (Func<bool>)(() => Commands.Darken),
			Setter = (Action<bool>)delegate(bool value)
			{
				Commands.Darken = value;
			}
		}, "ovr_darken", "Should overlays add background darkening?", "", ExecutionPolicy.PlayMode, "bool")
	};

	public static readonly List<(ArgumentConverter.ConvertDelegate, int)> ArgConverters = new List<(ArgumentConverter.ConvertDelegate, int)>();

	public static readonly List<(ArgumentConverter.PreprocessDelegate, int)> ArgPreprocessors = new List<(ArgumentConverter.PreprocessDelegate, int)>();
}
