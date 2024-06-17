using System;
using Epic.OnlineServices.Ecom;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EOSSDK;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using UnityEngine;

namespace Kingmaker.DLC;

[TypeId("819fc7622cd24ab5b30510b538d6b4ea")]
public class DlcStoreEpic : DlcStore, IDLCStoreEpic
{
	[SerializeField]
	private string m_EpicId;

	public string EpicId => m_EpicId;

	public override bool IsSuitable => StoreManager.Store == StoreType.EpicGames;

	public override IDLCStatus GetStatus()
	{
		bool flag = false;
		try
		{
			OwnershipStatus ownershipStatus = EpicGamesManager.DlcHelper.GetOwnershipStatus(m_EpicId);
			if (ownershipStatus == OwnershipStatus.Owned)
			{
				PFLog.System.Log($"DLC {base.OwnerBlueprint} is available through Epic (ID {m_EpicId}).");
				flag = true;
			}
			else
			{
				PFLog.System.Log($"DLC {base.OwnerBlueprint} is not available through Epic (ID {m_EpicId}, status {ownershipStatus}).");
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex, $"Failed to check DLC {base.OwnerBlueprint} availability on Steam (ID {m_EpicId}).");
		}
		if (!flag)
		{
			return null;
		}
		return DLCStatus.Available;
	}
}
