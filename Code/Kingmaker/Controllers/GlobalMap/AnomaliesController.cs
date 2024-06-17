using System.Linq;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Globalmap.SystemMap;

namespace Kingmaker.Controllers.GlobalMap;

public class AnomaliesController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		foreach (AnomalyEntityData item in Game.Instance.State.StarSystemObjects.Where((StarSystemObjectEntity obj) => obj is AnomalyEntityData))
		{
			item.View.Tick();
		}
	}
}
