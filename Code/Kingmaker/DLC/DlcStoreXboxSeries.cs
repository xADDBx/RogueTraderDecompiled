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

	public override bool AllowsPurchase => true;

	private string ExceptionMessage => $"Failed to check DLC {base.OwnerBlueprint} availability on XboxSeries (Store ID {m_StoreID}).";

	public override IDLCStatus GetStatus()
	{
		bool purchased = false;
		bool flag = false;
		return new DLCStatus
		{
			Purchased = purchased,
			DownloadState = (flag ? DownloadState.Loaded : DownloadState.NotLoaded),
			IsMounted = flag,
			IsEnabled = true
		};
	}
}
