using System.Threading.Tasks;
using Core.Async;
using Kingmaker.QA.Analytics;
using Kingmaker.Utility.Fsm;

namespace Kingmaker.Networking.NetGameFsm.States;

public class PlayingState : IStateAsync
{
	private readonly PhotonManager m_PhotonManager;

	private string m_RoomName;

	public PlayingState(PhotonManager photonManager)
	{
		m_PhotonManager = photonManager;
	}

	public async Task OnEnter()
	{
		await Awaiters.UnityThread;
		if (PhotonManager.Cheat.AllowRunWithOnePlayer || !m_PhotonManager.StopPlayingIfLastPlayer())
		{
			m_RoomName = m_PhotonManager.RoomName;
			OwlcatAnalytics.Instance.SendCoopStart(m_RoomName, m_PhotonManager.PlayerCount);
		}
	}

	public Task OnExit()
	{
		OwlcatAnalytics.Instance.SendCoopEnd(m_RoomName);
		return Task.CompletedTask;
	}
}
