using Kingmaker.Controllers.Interfaces;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Controllers.MapObjects;

internal class ScriptZoneController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		foreach (ScriptZoneEntity scriptZone in Game.Instance.State.ScriptZones)
		{
			scriptZone.Tick();
		}
	}
}
