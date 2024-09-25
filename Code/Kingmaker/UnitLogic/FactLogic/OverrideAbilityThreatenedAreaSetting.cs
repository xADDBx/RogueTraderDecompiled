using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("b1c174e0e5e64195a67810a635bee1de")]
public class OverrideAbilityThreatenedAreaSetting : UnitFactComponentDelegate, IHashable
{
	public RestrictionCalculator Restriction = new RestrictionCalculator();

	public BlueprintAbility.UsingInThreateningAreaType ThreatenedAreaRule;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartAbilitySettings>().Add(this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartAbilitySettings>()?.Remove(this);
	}

	public BlueprintAbility.UsingInThreateningAreaType? GetThreatenedAreaRule(AbilityData ability)
	{
		if (!Restriction.IsPassed(base.Fact, base.Context, null, ability))
		{
			return null;
		}
		return ThreatenedAreaRule;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
