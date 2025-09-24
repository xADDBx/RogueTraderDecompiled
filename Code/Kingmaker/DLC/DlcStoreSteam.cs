using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.ManualCoroutines;
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

	private CoroutineHandler? m_LoadingCoroutine;

	public uint SteamId => m_SteamId;

	public override bool IsSuitable => StoreManager.Store == StoreType.Steam;

	private string ExceptionMessage => $"Failed to check DLC {base.OwnerBlueprint} availability on Steam (ID {SteamId}).";

	public override bool TryGetStatus(out IDLCStatus value)
	{
		value = DLCStatus.UnAvailable;
		try
		{
			if (SteamManager.Initialized)
			{
				TryStopLoadingCoroutine();
				AppId_t appId_t = new AppId_t(SteamId);
				ulong punBytesDownloaded;
				ulong punBytesTotal;
				if (SteamApps.BIsDlcInstalled(appId_t))
				{
					value = DLCStatus.Available;
					PFLog.System.Log($"DLC {base.OwnerBlueprint} is available through Steam (ID {SteamId}).");
				}
				else if (SteamApps.GetDlcDownloadProgress(appId_t, out punBytesDownloaded, out punBytesTotal))
				{
					value = new DLCStatus
					{
						Purchased = true,
						DownloadState = ((punBytesDownloaded != 0L) ? DownloadState.Loading : DownloadState.NotLoaded),
						IsMounted = false
					};
					m_LoadingCoroutine = Game.Instance.CoroutinesController.InvokeInTime(StoreManager.RefreshAllDLCStatuses, 10.Seconds());
					PFLog.System.Log($"DLC {base.OwnerBlueprint} is available but not downloaded through Steam (ID {SteamId}).");
				}
				else
				{
					PFLog.System.Log($"DLC {base.OwnerBlueprint} is not available through Steam (ID {SteamId}).");
				}
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

	private void TryStopLoadingCoroutine()
	{
		if (m_LoadingCoroutine.HasValue)
		{
			Game.Instance.CoroutinesController.Stop(m_LoadingCoroutine.Value);
			m_LoadingCoroutine = null;
		}
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
