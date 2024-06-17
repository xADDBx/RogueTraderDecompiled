using System.Collections.Generic;
using Core.Cheats;
using Owlcat.Runtime.Core.Logging;

namespace Owlcat.Core.Overlays;

public static class Commands
{
	[Cheat(Name = "ovr_darken", Description = "Should overlays add background darkening?", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static bool Darken
	{
		get
		{
			return OverlayService.Instance?.DarkenBackground ?? false;
		}
		set
		{
			if (OverlayService.Instance != null)
			{
				OverlayService.Instance.DarkenBackground = value;
			}
		}
	}

	[Cheat(Name = "ovr_toggle", Description = "Toggle visibility of performance overlay by name", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void Toggle(string name)
	{
		OverlayService.Instance?.ToggleOverlay(name);
	}

	[Cheat(Name = "ovr_list", Description = "List all registered overlays", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void List()
	{
		IEnumerable<Overlay> enumerable = OverlayService.Instance?.All;
		LogChannel orCreate = LogChannelFactory.GetOrCreate("Console");
		if (enumerable == null)
		{
			return;
		}
		foreach (Overlay item in enumerable)
		{
			orCreate.Log(item.Name);
		}
	}

	[Cheat(Name = "ovr_list_graphs", Description = "List all graphs in the active overlay", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ListGraphs()
	{
		Overlay overlay = OverlayService.Instance?.Current;
		LogChannel orCreate = LogChannelFactory.GetOrCreate("Console");
		if ((overlay?.Graphs?.Count).GetValueOrDefault() == 0)
		{
			orCreate.Log("No graphs in current overlay");
			return;
		}
		foreach (Graph graph in overlay.Graphs)
		{
			orCreate.Log(graph.Name + ": " + (graph.Hidden ? "Hidden" : "Visible"));
		}
	}

	[Cheat(Name = "ovr_toggle_graph", Description = "Toggle visibility of a graph in currently visible by name", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ToggleGraph(string name)
	{
		OverlayService.Instance?.Current?.ToggleGraph(name);
	}
}
