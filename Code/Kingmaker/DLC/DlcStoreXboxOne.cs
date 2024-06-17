using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using UnityEngine;

namespace Kingmaker.DLC;

[TypeId("7c5c83d1b4e349e298a5c781cd7baad3")]
public class DlcStoreXboxOne : DlcStore, IDLCStoreXboxOne
{
	[SerializeField]
	private string m_IdentityName;

	[SerializeField]
	private string m_DisplayName;

	[SerializeField]
	private string m_ProductId;

	public string IdentityName => m_IdentityName;

	public string DisplayName => m_DisplayName;

	public string ProductId => m_ProductId;

	public override bool IsSuitable => StoreManager.Store == StoreType.XboxOne;

	public override bool AllowsPurchase => true;

	public override IDLCStatus GetStatus()
	{
		return null;
	}

	public override bool OpenShop()
	{
		return false;
	}

	public override bool Mount()
	{
		return false;
	}
}
