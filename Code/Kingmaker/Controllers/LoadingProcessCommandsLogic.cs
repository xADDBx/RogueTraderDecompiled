using System.Collections;
using Kingmaker.Networking;
using Kingmaker.Signals;
using UnityEngine;

namespace Kingmaker.Controllers;

public static class LoadingProcessCommandsLogic
{
	private const int TicksDelay = 9;

	private static void Tick()
	{
		NetworkingManager.ReceivePackets();
		Game.Instance.RealTimeController.Tick();
		if (Game.Instance.RealTimeController.IsSimulationTick)
		{
			Game.Instance.GameCommandQueue.Tick();
			Game.Instance.UnitCommandBuffer.Tick();
			Game.Instance.SynchronizedDataController.Tick();
			NetService.Instance.Tick();
		}
		Game.Instance.RealTimeController.FinishTick();
	}

	private static int Test(int ticksRemaining)
	{
		if (!Game.Instance.RealTimeController.IsSimulationTick)
		{
			return ticksRemaining;
		}
		if (Game.Instance.GameCommandQueue.HasScheduledCommands)
		{
			PFLog.System.Log($"Game commands has scheduled commands. Resetting ticks delay. TicksRemaining={ticksRemaining}->{9}");
			Game.Instance.GameCommandQueue.DumpScheduledCommands();
			return 9;
		}
		PFLog.System.Log($"Waiting for game commands in loading process. TicksRemaining={ticksRemaining - 1}");
		return ticksRemaining - 1;
	}

	public static IEnumerator WaitTickCommandsEnd()
	{
		float startTime = Time.realtimeSinceStartup;
		PFLog.System.Log($"Start tick commands in loading process at {startTime} sec. after startup");
		SignalWrapper signalWrapper = SignalService.Instance.RegisterNext();
		int ticksRemaining = 9;
		while (0 < ticksRemaining)
		{
			Tick();
			ticksRemaining = Test(ticksRemaining);
			yield return null;
		}
		PFLog.System.Log("Waiting for signal...");
		while (!SignalService.Instance.CheckReadyOrSend(ref signalWrapper))
		{
			Tick();
			yield return null;
		}
		PFLog.System.Log($"Stop tick game commands in loading process after {Time.realtimeSinceStartup - startTime}s. Dump scheduled commands and start load area process.");
		Game.Instance.GameCommandQueue.DumpScheduledCommands();
		Game.Instance.GameCommandQueue.CancelCurrentCommands();
	}
}
