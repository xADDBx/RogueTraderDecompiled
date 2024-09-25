using System.Threading.Tasks;
using Kingmaker.Utility.Fsm;

namespace Kingmaker.Networking.NetGameFsm.States;

public class InLobbyState : IStateAsync
{
	private PhotonManager m_PhotonManager;

	public InLobbyState(PhotonManager photonManager)
	{
		m_PhotonManager = photonManager;
	}

	public Task OnEnter()
	{
		LobbyNetManager.SetState(LobbyNetManager.State.InLobby);
		m_PhotonManager.OnJoinedLobby();
		return Task.CompletedTask;
	}

	public Task OnExit()
	{
		return Task.CompletedTask;
	}
}
