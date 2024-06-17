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

	public uint SteamId => m_SteamId;

	public override bool IsSuitable => StoreManager.Store == StoreType.Steam;

	public override IDLCStatus GetStatus()
	{
		bool flag = false;
		try
		{
			if (!flag && SteamManager.Initialized && SteamApps.BIsDlcInstalled(new AppId_t(SteamId)))
			{
				PFLog.System.Log($"DLC {base.OwnerBlueprint} is available through Steam (ID {SteamId}).");
				flag = true;
			}
			if (!flag)
			{
				PFLog.System.Log($"DLC {base.OwnerBlueprint} is not available through Steam (ID {SteamId}).");
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex, $"Failed to check DLC {base.OwnerBlueprint} availability on Steam (ID {SteamId}).");
		}
		if (!flag)
		{
			return null;
		}
		return DLCStatus.Available;
	}
}
