using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("72abaa04158a4af498ec61527311b1bc")]
public class UpdateBuffEndCondition : MechanicEntityFactComponentDelegate, IHashable
{
	[SerializeField]
	private BuffEndCondition m_EndCondition = BuffEndCondition.CombatEnd;

	protected override void OnApplyPostLoadFixes()
	{
		if (base.Fact is Buff buff && buff.EndCondition != m_EndCondition)
		{
			buff.EndCondition = m_EndCondition;
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
