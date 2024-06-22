using Kingmaker.Controllers.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Controllers;

public class PauseController : IControllerTick, IController, IControllerReset
{
	private NetPlayerGroup m_PausedPlayer = NetPlayerGroup.Empty;

	private bool m_ManualPause;

	private bool m_Update = true;

	public bool IsManualPause => m_ManualPause;

	private static NetPlayerGroup PlayersReadyMask => NetworkingManager.PlayersReadyMask;

	public bool IsPausedByPlayers => m_PausedPlayer.Contains(PlayersReadyMask);

	public bool IsPausedByLocalPlayer => m_PausedPlayer.Contains(NetworkingManager.LocalNetPlayer);

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		if (m_Update)
		{
			m_Update = false;
			Game.Instance.SetIsPauseForce(m_ManualPause || IsPausedByPlayers);
			EventBus.RaiseEvent(delegate(IPauseHandler h)
			{
				h.OnPauseToggled();
			});
		}
	}

	void IControllerReset.OnReset()
	{
		m_PausedPlayer = NetPlayerGroup.Empty;
		m_ManualPause = false;
		m_Update = true;
	}

	public void RequestPauseUi(bool isPaused)
	{
		if (isPaused != IsPausedByLocalPlayer)
		{
			Game.Instance.GameCommandQueue.RequestPauseUi(isPaused);
		}
	}

	public void SetPlayer(NetPlayer player, bool isPaused)
	{
		NetPlayerGroup netPlayerGroup = (isPaused ? m_PausedPlayer.Add(player) : m_PausedPlayer.Del(player));
		m_Update = m_Update || !m_PausedPlayer.Equals(netPlayerGroup);
		m_PausedPlayer = netPlayerGroup;
	}

	public void SetManualPause(bool isPaused)
	{
		if (!IsPausedByPlayers)
		{
			m_ManualPause = isPaused;
			m_Update = true;
		}
	}

	public void OnPlayerLeftRoom()
	{
		m_Update = true;
	}
}
