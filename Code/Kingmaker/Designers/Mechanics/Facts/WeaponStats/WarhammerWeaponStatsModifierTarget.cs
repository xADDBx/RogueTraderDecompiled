using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.WeaponStats;

[Serializable]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("66c309f734dd49adb53a6ee84985a049")]
public class WarhammerWeaponStatsModifierTarget : WarhammerWeaponStatsModifier, ITargetRulebookHandler<RuleCalculateStatsWeapon>, IRulebookHandler<RuleCalculateStatsWeapon>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public void OnEventAboutToTrigger(RuleCalculateStatsWeapon rule)
	{
		if (Restrictions.IsPassed(new MechanicsContext(rule.InitiatorUnit, base.Fact.ConcreteOwner, rule.Ability.Blueprint), base.Fact.ConcreteOwner, base.Fact))
		{
			Apply(rule);
		}
	}

	public void OnEventDidTrigger(RuleCalculateStatsWeapon evt)
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
