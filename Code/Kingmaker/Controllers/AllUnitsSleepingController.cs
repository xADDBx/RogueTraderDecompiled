using Kingmaker.Controllers.Interfaces;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Controllers;

public class AllUnitsSleepingController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		Game.Instance.State.ClearAwakeUnits();
		foreach (AbstractUnitEntity item in Game.Instance.State.AllUnits.All)
		{
			if (item != Game.Instance.Player.PlayerShip)
			{
				item.IsSleeping = true;
			}
		}
		Game.Instance.ReadyForCombatUnitGroups.Clear();
	}
}
