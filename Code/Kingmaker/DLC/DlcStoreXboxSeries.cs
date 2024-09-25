using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using UnityEngine;

namespace Kingmaker.DLC;

[TypeId("afa9f9b05919463191ecefd722586a8c")]
public class DlcStoreXboxSeries : DlcStore, IDLCStoreXboxSeries
{
	[SerializeField]
	private string m_StoreID = "XXXXXXXXXXXX";

	public override bool IsSuitable => StoreManager.Store == StoreType.XboxSeries;

	public override bool AllowsInstalling => true;

	public override bool AllowsDeleting => true;

	private string ExceptionMessage => $"Failed to check DLC {base.OwnerBlueprint} availability on XboxSeries (Store ID {m_StoreID}).";

	public override bool TryGetStatus(out IDLCStatus value)
	{
		bool purchased = false;
		bool flag = false;
		bool flag2 = false;
		value = new DLCStatus
		{
			Purchased = purchased,
			DownloadState = (flag ? DownloadState.Loaded : (flag2 ? DownloadState.Loading : DownloadState.NotLoaded)),
			IsMounted = flag
		};
		return true;
	}

	public override bool OpenShop()
	{
		_ = IsSuitable;
		return false;
	}

	public override bool Mount()
	{
		_ = IsSuitable;
		return false;
	}

	public override bool Install()
	{
		return true;
	}

	public override bool Delete()
	{
		return true;
	}
}
