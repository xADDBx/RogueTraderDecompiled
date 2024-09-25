using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using UnityEngine;

namespace Kingmaker.DLC;

[TypeId("a0d17b1e44ce4214bb7ca1af5ac5c0bb")]
public class DlcStorePS4 : DlcStore, IDLCStorePS4
{
	[SerializeField]
	private int m_ServiceLabel;

	[SerializeField]
	private string m_EntitlementLabel = "ZZZZZZZZZZZZZZZZ";

	public int ServiceLabel => m_ServiceLabel;

	public string EntitlementLabel => m_EntitlementLabel;

	public override bool IsSuitable => StoreManager.Store == StoreType.PS4;

	private string ExceptionMessage => $"Failed to check DLC {base.OwnerBlueprint} availability on PS4 (ServiceLabel {ServiceLabel.ToString()}, ProductLabel {EntitlementLabel}).";

	public override bool TryGetStatus(out IDLCStatus value)
	{
		bool purchased = false;
		bool flag = false;
		value = new DLCStatus
		{
			Purchased = purchased,
			DownloadState = (flag ? DownloadState.Loaded : DownloadState.NotLoaded),
			IsMounted = flag
		};
		return true;
	}

	public override bool OpenShop()
	{
		bool result = false;
		if (!IsSuitable)
		{
			return false;
		}
		return result;
	}

	public override bool Mount()
	{
		bool result = false;
		if (!IsSuitable)
		{
			return false;
		}
		return result;
	}
}
