using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("9dd5c8ea0cade7141b71b41fb84fa1e2")]
public class WarhammerHitChancePenalty : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public bool SpecificRangeType;

	[ShowIf("SpecificRangeType")]
	public WeaponRangeType WeaponRangeType;

	public ContextValue Value;

	public ContextValue Multiplier;

	public void OnEventAboutToTrigger(RuleCalculateHitChances evt)
	{
		ItemEntityWeapon weapon = evt.Ability.Weapon;
		if (!SpecificRangeType || (weapon != null && WeaponRangeType.IsSuitableWeapon(weapon)))
		{
			evt.HitChanceValueModifiers.Add(-Value.Calculate(base.Context) * Multiplier.Calculate(base.Context), base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateHitChances evt)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
