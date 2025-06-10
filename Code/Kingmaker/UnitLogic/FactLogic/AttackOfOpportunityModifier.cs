using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("282b5c4c4e134d809708701fd45e28c5")]
public class AttackOfOpportunityModifier : UnitFactComponentDelegate, IHashable
{
	[FormerlySerializedAs("EnableAndPrioritirizeRangedAttack")]
	[FormerlySerializedAs("CanUseRanged")]
	public bool EnableAndPrioritizeRangedAttack = true;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<UnitPartAttackOfOpportunityModifier>().Add(base.Fact, this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<UnitPartAttackOfOpportunityModifier>()?.Remove(base.Fact, this);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
