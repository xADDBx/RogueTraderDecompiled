using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.View;

namespace Kingmaker.Controllers.UnityEventsReplacements;

public class UnitMovementController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		List<UnitMovementAgentBase> allAgents = UnitMovementAgentBase.AllAgents;
		int i = 0;
		for (int count = allAgents.Count; i < count; i++)
		{
			allAgents[i].Tick();
		}
		AstarPath active = AstarPath.active;
		if (!(active != null) || !Game.Instance.CurrentlyLoadedArea.IsNavmeshArea)
		{
			return;
		}
		foreach (UpdateHook instance in UpdateHook.Instances)
		{
			if (instance != null)
			{
				instance.Tick();
			}
		}
		active.Tick();
	}
}
