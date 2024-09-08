using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Steamworks;
using UnityEngine;

namespace Kingmaker.DLC;

[TypeId("0b27d8bd19d4426eafd38449d5d9aec7")]
public class DlcStoreSteam : DlcStore, IDLCStoreSteam
{
	[SerializeField]
	private uint m_SteamId;

	[SerializeField]
	private string m_ShopLink = "https://store.steampowered.com/app/2186680/Warhammer_40000_Rogue_Trader/";

	public uint SteamId => m_SteamId;

	public override bool IsSuitable => StoreManager.Store == StoreType.Steam;

	private string ExceptionMessage => $"Failed to check DLC {base.OwnerBlueprint} availability on Steam (ID {SteamId}).";

	public override bool TryGetStatus(out IDLCStatus value)
	{
		value = DLCStatus.UnAvailable;
		try
		{
			if (SteamManager.Initialized && SteamApps.BIsDlcInstalled(new AppId_t(SteamId)))
			{
				value = DLCStatus.Available;
				PFLog.System.Log($"DLC {base.OwnerBlueprint} is available through Steam (ID {SteamId}).");
			}
			else
			{
				PFLog.System.Log($"DLC {base.OwnerBlueprint} is not available through Steam (ID {SteamId}).");
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex, ExceptionMessage);
		}
		return true;
	}

	public override bool OpenShop()
	{
		if (!IsSuitable)
		{
			return false;
		}
		bool flag = false;
		try
		{
			if (SteamManager.Initialized && SteamUtils.IsOverlayEnabled())
			{
				SteamFriends.ActivateGameOverlayToStore(new AppId_t(SteamId), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
				flag = true;
			}
			if (!flag)
			{
				Application.OpenURL(m_ShopLink);
				flag = true;
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex, ExceptionMessage);
		}
		return flag;
	}
}
