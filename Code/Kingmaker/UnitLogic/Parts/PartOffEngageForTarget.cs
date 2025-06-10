using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartOffEngageForTarget : BaseUnitPart, IHashable
{
	private EntityRef<BaseUnitEntity> m_TargetRef;

	public void SetTarget(BaseUnitEntity target)
	{
		m_TargetRef = new EntityRef<BaseUnitEntity>(target);
		if (target == null)
		{
			RemoveSelf();
		}
	}

	public bool IsOffEngageForTarget(BaseUnitEntity target)
	{
		if (!m_TargetRef.IsNull)
		{
			return m_TargetRef.Entity == target;
		}
		return false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
