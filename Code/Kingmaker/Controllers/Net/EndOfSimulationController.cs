using Kingmaker.Controllers.Interfaces;
using Kingmaker.Replay;

namespace Kingmaker.Controllers.Net;

public class EndOfSimulationController : IControllerTick, IController
{
	TickType IControllerTick.GetTickType()
	{
		return TickType.Any;
	}

	void IControllerTick.Tick()
	{
		Kingmaker.Replay.Replay.SaveState();
	}
}
