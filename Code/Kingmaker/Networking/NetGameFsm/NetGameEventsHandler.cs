using System;
using Core.Async;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Fsm;

namespace Kingmaker.Networking.NetGameFsm;

public class NetGameEventsHandler : StateMachine<NetGame.State, NetGame.Trigger>.IStateMachineEventsHandler
{
	public event Action<NetGame.State, NetGame.State> StateChanged;

	public void OnFireTrigger(NetGame.Trigger trigger)
	{
		PFLog.Net.Log($"[NetGame][FSM] Fire: {trigger}");
	}

	public async void OnStateChanged(NetGame.State oldState, NetGame.State newState)
	{
		PFLog.Net.Log($"[NetGame][FSM] State changed: {oldState} -> {newState}");
		try
		{
			await Awaiters.UnityThread;
			EventBus.RaiseEvent(delegate(INetEvents h)
			{
				h.HandleNetGameStateChanged(newState);
			});
			this.StateChanged?.Invoke(oldState, newState);
		}
		catch (Exception ex)
		{
			PFLog.Net.Exception(ex);
		}
	}

	public void OnProcessTrigger(NetGame.Trigger trigger, NetGame.State currentState, NetGame.State nextState)
	{
		PFLog.Net.Log($"[NetGame][FSM] Process: {trigger}, {currentState} -> {nextState}");
	}

	public void OnFireException(Exception exception)
	{
		PFLog.Net.Exception(exception);
	}

	public void OnUnhandledTransition(NetGame.Trigger trigger, NetGame.State currentState)
	{
		PFLog.Net.Error($"[NetGame][FSM] Unhandled transition: from {currentState} by {trigger} trigger");
	}

	public void OnIgnoreTrigger(NetGame.Trigger trigger, NetGame.State currentState)
	{
		PFLog.Net.Log($"[NetGame][FSM] Ignore trigger {trigger} from {currentState}");
	}
}
