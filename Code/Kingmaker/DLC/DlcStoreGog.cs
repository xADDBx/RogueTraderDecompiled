using System;
using Galaxy.Api;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using UnityEngine;

namespace Kingmaker.DLC;

[TypeId("db1a408708c04a8ca9b4af36cdb3df8e")]
public class DlcStoreGog : DlcStore, IDLCStoreGog
{
	[SerializeField]
	private ulong m_GogId;

	[SerializeField]
	private string m_ShopLink = "https://www.gog.com/en/game/warhammer_40000_rogue_trader/";

	public ulong GogId => m_GogId;

	public override bool IsSuitable => StoreManager.Store == StoreType.GoG;

	private string ExceptionMessage => $"Failed to check DLC {base.OwnerBlueprint} availability on GOG (ID {GogId}).";

	public override IDLCStatus GetStatus()
	{
		bool flag = false;
		try
		{
			if (!flag)
			{
				IApps apps = GalaxyInstance.Apps();
				if (apps != null && apps.IsDlcInstalled(GogId))
				{
					PFLog.System.Log($"DLC {base.OwnerBlueprint} is available through GOG Galaxy (ID {GogId}).");
					flag = true;
				}
			}
			if (!flag)
			{
				PFLog.System.Log($"DLC {base.OwnerBlueprint} is not available through GOG (ID {GogId}).");
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex, $"Failed to check DLC {base.OwnerBlueprint} availability on GOG (ID {GogId}).");
		}
		if (!flag)
		{
			return null;
		}
		return DLCStatus.Available;
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
