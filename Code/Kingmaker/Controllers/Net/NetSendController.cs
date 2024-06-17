using Kingmaker.Controllers.Interfaces;
using Kingmaker.Networking;

namespace Kingmaker.Controllers.Net;

public class NetSendController : IControllerTick, IController, IControllerStart, IControllerStop
{
	void IControllerStart.OnStart()
	{
	}

	void IControllerStop.OnStop()
	{
		Game.Instance.GameCommandQueue.DumpScheduledCommands();
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Network;
	}

	void IControllerTick.Tick()
	{
		NetService.Instance.Tick();
	}
}
