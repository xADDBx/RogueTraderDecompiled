using System;
using System.Collections.Generic;
using Kingmaker.GameModes;
using Kingmaker.Networking;
using Kingmaker.Networking.Player;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Photon.Realtime;

namespace Kingmaker.UI.MVVM.VM.NetRoles;

public class NetRolesVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INetLobbyPlayersHandler, ISubscriber, IAreaHandler, IGameModeHandler
{
	private readonly Action m_CloseAction;

	public readonly List<NetRolesPlayerVM> PlayerVms = new List<NetRolesPlayerVM>();

	public readonly bool IsRoomOwner;

	public NetRolesVM(Action closeAction)
	{
		AddDisposable(EventBus.Subscribe(this));
		m_CloseAction = closeAction;
		ReadonlyList<PlayerInfo> activePlayers = PhotonManager.Instance.ActivePlayers;
		IsRoomOwner = PhotonManager.Instance.IsRoomOwner;
		foreach (PlayerInfo item in activePlayers)
		{
			NetRolesPlayerVM netRolesPlayerVM = new NetRolesPlayerVM();
			netRolesPlayerVM.SetPlayer(item.Player, item.UserId, item.IsActive);
			PlayerVms.Add(netRolesPlayerVM);
		}
	}

	protected override void DisposeImplementation()
	{
	}

	public void OnClose()
	{
		PlayerVms.ForEach(delegate(NetRolesPlayerVM p)
		{
			p.Dispose();
		});
		PlayerVms.Clear();
		m_CloseAction?.Invoke();
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		OnClose();
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.Dialog)
		{
			OnClose();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void OnAreaBeginUnloading()
	{
		OnClose();
	}

	public void OnAreaDidLoad()
	{
	}
}
