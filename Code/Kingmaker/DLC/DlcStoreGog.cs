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

	public ulong GogId => m_GogId;

	public override bool IsSuitable => StoreManager.Store == StoreType.GoG;

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
}
