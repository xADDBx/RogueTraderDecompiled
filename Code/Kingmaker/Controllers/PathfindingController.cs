using Kingmaker.Controllers.Interfaces;
using Kingmaker.Pathfinding;
using Pathfinding;

namespace Kingmaker.Controllers;

public class PathfindingController : IControllerTick, IController
{
	private const long PathLifeMs = 120000L;

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		PathPool.TrimActiveList(Game.Instance.Player.GameTime.Ticks, 1200000000L);
		PathfindingService.Instance.Tick();
	}
}
