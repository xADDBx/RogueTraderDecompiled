using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("717c35726c484dc9a65b339090ea03d7")]
public class TacticalAdvantageBasedBuff : UnitBuffComponentDelegate, IHashable
{
	[SerializeField]
	private BlueprintFeatureReference m_ReductionFeature;

	[SerializeField]
	private BlueprintBuffReference m_TacticalAdvantageBuff;

	public BlueprintFeature ReductionFeature => m_ReductionFeature?.Get();

	public BlueprintBuff TacticalAdvantageBuff => m_TacticalAdvantageBuff?.Get();

	protected override void OnActivate()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		if (maybeCaster != null)
		{
			Buff buff = maybeCaster.Buffs.GetBuff(TacticalAdvantageBuff);
			if (buff != null)
			{
				int count = (maybeCaster.Facts.Contains(ReductionFeature) ? ((buff.GetRank() + 1) * 50 / 100) : ((buff.GetRank() + 3) * 25 / 100));
				buff.RemoveRank(count);
			}
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
