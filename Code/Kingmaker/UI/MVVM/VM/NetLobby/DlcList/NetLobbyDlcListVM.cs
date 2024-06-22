using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.DLC;
using Kingmaker.Networking;
using Kingmaker.Networking.Player;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM.VM.NetLobby.DlcList;

public class NetLobbyDlcListVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly List<NetLobbyDlcListDlcEntityVM> Dlcs = new List<NetLobbyDlcListDlcEntityVM>();

	private readonly Action m_CloseAction;

	public readonly List<string> PlayerNames = new List<string>();

	public NetLobbyDlcListVM(Action closeAction, List<IBlueprintDlc> hostDlcs)
	{
		m_CloseAction = closeAction;
		ReadonlyList<PlayerInfo> allPlayers = PhotonManager.Instance.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			PlayerNames.Add((PhotonManager.Player.GetNickName(allPlayers[i].Player, out var nickName) && !string.IsNullOrWhiteSpace(nickName)) ? nickName : allPlayers[i].UserId);
		}
		foreach (BlueprintDlc item in (from dlc in hostDlcs.OfType<BlueprintDlc>()
			orderby dlc.DlcType
			select dlc).ToList())
		{
			Dlcs.Add(new NetLobbyDlcListDlcEntityVM(item));
		}
	}

	protected override void DisposeImplementation()
	{
		PlayerNames?.Clear();
		Dlcs?.Clear();
	}

	public void CloseWindow()
	{
		m_CloseAction?.Invoke();
	}
}
