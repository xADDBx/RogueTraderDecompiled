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

	[SerializeField]
	private string m_ShopLink = "https://store.epicgames.com/en-US/p/warhammer-40000-rogue-trader/";

	public string EpicId => m_EpicId;

	public override bool IsSuitable => StoreManager.Store == StoreType.EpicGames;

	private string ExceptionMessage => $"Failed to check DLC {base.OwnerBlueprint} availability on Steam (ID {m_EpicId}).";

	public override IDLCStatus GetStatus()
	{
		IDLCStatus result = null;
		try
		{
			OwnershipStatus ownershipStatus = EpicGamesManager.DlcHelper.GetOwnershipStatus(m_EpicId);
			if (ownershipStatus == OwnershipStatus.Owned)
			{
				result = DLCStatus.Available;
				PFLog.System.Log($"DLC {base.OwnerBlueprint} is available through Epic (ID {m_EpicId}).");
			}
			else
			{
				PFLog.System.Log($"DLC {base.OwnerBlueprint} is not available through Epic (ID {m_EpicId}, status {ownershipStatus}).");
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex, ExceptionMessage);
		}
		return result;
	}

	public override bool OpenShop()
	{
		if (!IsSuitable)
		{
			return false;
		}
		bool result = false;
		try
		{
			Application.OpenURL(m_ShopLink);
			result = true;
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex, ExceptionMessage);
		}
		return result;
	}
}
