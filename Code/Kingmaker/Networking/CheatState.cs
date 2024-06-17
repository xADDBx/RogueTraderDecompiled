using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Networking;

public class CheatState
{
	private bool m_AllowRunWithOnePlayer;

	public bool AllowRunWithOnePlayer
	{
		get
		{
			return m_AllowRunWithOnePlayer;
		}
		set
		{
			m_AllowRunWithOnePlayer = value;
			if (m_AllowRunWithOnePlayer)
			{
				EventBus.RaiseEvent(delegate(INetLobbyPlayersHandler h)
				{
					h.HandlePlayerEnteredRoom(null);
				});
			}
			else
			{
				EventBus.RaiseEvent(delegate(INetLobbyPlayersHandler h)
				{
					h.HandlePlayerLeftRoom(null);
				});
			}
			PFLog.Net.Log("[CheatState.AllowRunWithOnePlayer] " + ((!value) ? "not" : "") + " allowed");
		}
	}
}
