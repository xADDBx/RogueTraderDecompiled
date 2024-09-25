using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using UnityEngine;

namespace Kingmaker.DLC;

[TypeId("9a44845238e543cf8e31669b35b323bc")]
public class DlcStorePS5 : DlcStore, IDLCStorePS5
{
	[SerializeField]
	private string m_EntitlementLabel = "ZZZZZZZZZZZZZZZZ";

	public string EntitlementLabel => m_EntitlementLabel;

	public override bool IsSuitable => StoreManager.Store == StoreType.PS5;

	public override bool AllowsInstalling => true;

	public override bool AllowsDeleting => true;

	private string ExceptionMessage => $"Failed to check DLC {base.OwnerBlueprint} availability on PS5 (ProductLabel {EntitlementLabel}).";

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

	public override bool Install()
	{
		return true;
	}

	public override bool Delete()
	{
		return true;
	}
}
