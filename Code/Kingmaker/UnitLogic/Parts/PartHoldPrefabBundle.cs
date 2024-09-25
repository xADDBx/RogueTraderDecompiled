using Kingmaker.ResourceManagement;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartHoldPrefabBundle : AbstractUnitPart, IHashable
{
	private BundledResourceHandle<UnitEntityView> m_Handle;

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		if (m_Handle == null)
		{
			m_Handle = BundledResourceHandle<UnitEntityView>.Request(base.Owner.Blueprint.Prefab.AssetId, hold: true);
		}
	}

	protected override void OnAttachOrPostLoad()
	{
		base.OnAttachOrPostLoad();
		if (m_Handle == null)
		{
			m_Handle = BundledResourceHandle<UnitEntityView>.Request(base.Owner.Blueprint.Prefab.AssetId, hold: true);
		}
	}

	protected override void OnViewWillDetach()
	{
		m_Handle?.Dispose();
		m_Handle = null;
		base.OnViewWillDetach();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
