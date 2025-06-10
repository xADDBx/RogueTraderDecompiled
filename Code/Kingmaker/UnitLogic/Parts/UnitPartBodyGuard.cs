using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartBodyGuard : EntityPart<AbstractUnitEntity>, IHashable
{
	private bool m_Initialized;

	private EntityRef<BaseUnitEntity> m_DefendantRef;

	public BaseUnitEntity Defendant => m_DefendantRef;

	public void Init(BaseUnitEntity defendant)
	{
		m_DefendantRef = defendant;
		if (defendant == null)
		{
			PFLog.Default.Error("UnitPartBodyGuard.Init: Defendant is null");
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
