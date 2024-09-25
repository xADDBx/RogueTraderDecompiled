using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.DLC;
using Kingmaker.Networking;
using Kingmaker.Networking.Player;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM.VM.NetLobby.DlcList;

public class NetLobbyDlcListDlcEntityVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public enum DlcContainsTypeEnum
	{
		Contains,
		Sharing,
		HasNo
	}

	public readonly string DlcName;

	public readonly List<DlcContainsTypeEnum> PlayersHasDlcType = new List<DlcContainsTypeEnum>();

	public NetLobbyDlcListDlcEntityVM(IBlueprintDlc dlc)
	{
		BlueprintDlc blueprintDlc = dlc as BlueprintDlc;
		DlcName = ((blueprintDlc != null) ? blueprintDlc.GetDlcName() : string.Empty);
		List<PlayerInfo> list = PhotonManager.Instance.AllPlayers.ToList();
		PlayerInfo playerInfo = list.FirstOrDefault((PlayerInfo p) => p.Player.ActorNumber == PhotonManager.Instance.MasterClientId);
		foreach (PlayerInfo item in list)
		{
			if (item.UserId == playerInfo.UserId)
			{
				continue;
			}
			PhotonManager.DLC.TryGetPlayerDLC(item.UserId, out var playerDLCs);
			if (playerDLCs.Contains(dlc))
			{
				PlayersHasDlcType.Add(DlcContainsTypeEnum.Contains);
				continue;
			}
			DlcTypeEnum? dlcTypeEnum = blueprintDlc?.DlcType;
			bool flag;
			if (dlcTypeEnum.HasValue)
			{
				DlcTypeEnum valueOrDefault = dlcTypeEnum.GetValueOrDefault();
				if ((uint)(valueOrDefault - 2) <= 1u)
				{
					flag = true;
					goto IL_00fa;
				}
			}
			flag = false;
			goto IL_00fa;
			IL_00fa:
			if (flag)
			{
				PlayersHasDlcType.Add(DlcContainsTypeEnum.Sharing);
			}
			else
			{
				PlayersHasDlcType.Add(DlcContainsTypeEnum.HasNo);
			}
		}
	}

	protected override void DisposeImplementation()
	{
		PlayersHasDlcType?.Clear();
	}
}
