using Kingmaker.Controllers.Interfaces;
using Owlcat.Runtime.Visual.FogOfWar;

namespace Kingmaker.Controllers;

public class FogOfWarBlockerController : IControllerTick, IController
{
	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		FogOfWarBlockerUpdater.ManualUpdate();
	}
}
