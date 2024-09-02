using Kingmaker.Controllers.Interfaces;

namespace Kingmaker.Controllers;

public sealed class StabilizeInterpolationController : IControllerTick, IController
{
	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		Game.Instance.InterpolationController.Tick(1f);
	}
}
