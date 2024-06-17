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
[TypeId("603ec7b842284a9ca2db7783ec9365ba")]
public class WarhammerHitChanceBonusTarget : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public ContextValue Value;

	public bool SpecificRangeType;

	[ShowIf("SpecificRangeType")]
	public WeaponRangeType WeaponRangeType;

	public void OnEventAboutToTrigger(RuleCalculateHitChances evt)
	{
		ItemEntityWeapon weapon = evt.Ability.Weapon;
		if (!SpecificRangeType || (weapon != null && WeaponRangeType.IsSuitableWeapon(weapon)))
		{
			evt.HitChanceValueModifiers.Add(Value.Calculate(base.Context), base.Fact);
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
