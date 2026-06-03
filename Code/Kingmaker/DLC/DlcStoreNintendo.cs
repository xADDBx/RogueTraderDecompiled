using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Console.NintendoSwitch2;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using UnityEngine;

namespace Kingmaker.DLC;

[TypeId("6a037efccb5e47f080a09b1074258b22")]
public class DlcStoreNintendo : DlcStore
{
	[SerializeField]
	private int m_AocIndex = -1;

	[SerializeField]
	private string m_AocUId = "";

	[SerializeField]
	private int m_ReleaseVersion;

	private static int[] s_AocTempBuffer;

	private static int s_AocCount;

	public int AocIndex => m_AocIndex;

	public string AocUId => m_AocUId;

	public int ReleaseVersion => m_ReleaseVersion;

	public override bool AllowsDeleting => true;

	public override bool IsSuitable => StoreManager.Store == StoreType.Nintendo;

	public override bool TryGetStatus(out IDLCStatus value)
	{
		Switch2DlcFileSystem.Update();
		value = Switch2DlcFileSystem.GetStatusById(m_AocIndex);
		return true;
	}

	public override bool OpenShop()
	{
		return base.OpenShop();
	}
}
