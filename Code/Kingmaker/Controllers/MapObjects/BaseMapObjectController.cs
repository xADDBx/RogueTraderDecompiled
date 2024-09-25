using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Controllers.MapObjects;

public abstract class BaseMapObjectController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (!ShouldTick())
		{
			return;
		}
		foreach (MapObjectEntity mapObject in Game.Instance.State.MapObjects)
		{
			TickOnMapObject(mapObject);
		}
	}

	protected virtual bool ShouldTick()
	{
		return true;
	}

	protected abstract void TickOnMapObject(MapObjectEntity mapObject);
}
