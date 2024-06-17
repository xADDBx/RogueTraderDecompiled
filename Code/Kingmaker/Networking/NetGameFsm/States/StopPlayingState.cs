using System.Threading.Tasks;
using Core.Async;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Fsm;

namespace Kingmaker.Networking.NetGameFsm.States;

public class StopPlayingState : IStateAsync
{
	private readonly bool m_ShouldLeaveLobby;

	private readonly PhotonManager m_PhotonManager;

	private readonly INetGame m_NetGame;

	public StopPlayingState(bool shouldLeaveLobby, INetGame netGame, PhotonManager photonManager)
	{
		m_ShouldLeaveLobby = shouldLeaveLobby;
		m_PhotonManager = photonManager;
		m_NetGame = netGame;
	}

	public async Task OnEnter()
	{
		if (m_ShouldLeaveLobby)
		{
			await m_PhotonManager.LeaveRoomAsync();
		}
		await Awaiters.UnityThread;
		m_PhotonManager.ClearState(m_ShouldLeaveLobby);
		await WaitPhotonStableState();
		PFLog.Net.Log("[StopPlayingState.OnEnter] end");
		m_NetGame.OnPlayingStopped();
	}

	public async Task OnExit()
	{
		if (m_ShouldLeaveLobby)
		{
			await Awaiters.UnityThread;
			EventBus.RaiseEvent(delegate(INetStopPlayingHandler h)
			{
				h.HandleStopPlaying();
			});
		}
	}

	private async Task WaitPhotonStableState()
	{
		PFLog.Net.Log("[StopPlayingState.WaitPhotonStableState]");
		while (!m_PhotonManager.IsInStableState)
		{
			await Task.Yield();
		}
	}
}
