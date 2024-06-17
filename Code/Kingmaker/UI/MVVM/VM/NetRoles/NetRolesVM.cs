using System;
using System.Collections.Generic;
using Kingmaker.Networking;
using Kingmaker.Networking.Player;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM.VM.NetRoles;

public class NetRolesVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly Action m_CloseAction;

	public readonly List<NetRolesPlayerVM> PlayerVms = new List<NetRolesPlayerVM>();

	public readonly bool IsRoomOwner;

	public NetRolesVM(Action closeAction)
	{
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
}
