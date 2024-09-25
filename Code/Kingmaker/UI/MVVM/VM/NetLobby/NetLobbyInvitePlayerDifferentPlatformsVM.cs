using System;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM.VM.NetLobby;

public class NetLobbyInvitePlayerDifferentPlatformsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly Action m_CloseAction;

	private readonly Action m_InvitePlayer;

	private readonly Action m_InviteEpicGamesPlayer;

	public NetLobbyInvitePlayerDifferentPlatformsVM(Action closeAction, Action invitePlayerAction, Action inviteEpicGamesPlayerAction)
	{
		m_CloseAction = closeAction;
		m_InvitePlayer = invitePlayerAction;
		m_InviteEpicGamesPlayer = inviteEpicGamesPlayerAction;
	}

	protected override void DisposeImplementation()
	{
	}

	public void OnClose()
	{
		m_CloseAction();
	}

	public void OnInvitePlayer()
	{
		m_InvitePlayer();
	}

	public void OnInviteEpicGamesPlayer()
	{
		m_InviteEpicGamesPlayer();
	}
}
