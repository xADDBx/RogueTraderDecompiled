using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Stores.DlcInterfaces;
using UnityEngine;

namespace Kingmaker.DLC;

[TypeId("687dcfbd9b3f4793b748af4e1781b817")]
[AllowedOn(typeof(BlueprintDlc))]
public abstract class DlcStore : BlueprintComponent, IDlcStore
{
	[SerializeField]
	private bool m_ComingSoon;

	public abstract bool IsSuitable { get; }

	public virtual bool AllowsPurchase => false;

	public bool ComingSoon => m_ComingSoon;

	public virtual IDLCStatus GetStatus()
	{
		return null;
	}

	public virtual bool OpenShop()
	{
		return false;
	}

	public virtual bool Mount()
	{
		return false;
	}
}
