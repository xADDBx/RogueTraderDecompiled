using System;
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
		if (0 == 0)
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
