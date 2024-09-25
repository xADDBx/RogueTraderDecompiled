using System;
using System.Diagnostics;
using Kingmaker.Networking.NetGameFsm;

namespace Kingmaker.Networking;

public class IdleConnectionChecker
{
	private static TimeSpan DisconnectTime = TimeSpan.FromSeconds(60.0);

	private readonly NetGame m_NetGame;

	private readonly Stopwatch m_Stopwatch = Stopwatch.StartNew();

	private bool m_LobbyViewOpened;

	public bool LobbyViewOpened
	{
		get
		{
			return m_LobbyViewOpened;
		}
		set
		{
			m_LobbyViewOpened = value;
			if (!m_LobbyViewOpened)
			{
				m_Stopwatch.Restart();
			}
		}
	}

	public IdleConnectionChecker(NetGame netGame)
	{
		m_NetGame = netGame;
		m_NetGame.EventsHandler.StateChanged += OnStateChanged;
	}

	private void OnStateChanged(NetGame.State oldState, NetGame.State newState)
	{
		m_Stopwatch.Restart();
	}

	public bool ShouldDisconnect()
	{
		if (!LobbyViewOpened && PhotonManager.NetGame.CurrentState == NetGame.State.NetInitialized)
		{
			return m_Stopwatch.Elapsed > DisconnectTime;
		}
		return false;
	}
}
