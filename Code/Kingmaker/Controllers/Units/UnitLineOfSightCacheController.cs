using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Controllers.Units;

public class UnitLineOfSightCacheController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		for (int i = 0; i < Game.Instance.State.AllBaseAwakeUnits.Count; i++)
		{
			Game.Instance.State.AllBaseAwakeUnits[i].Vision.SightCache.ClearIfNeeded();
		}
	}
}
