using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.WeaponStats;

[Serializable]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("9f491e61b52742b2b04cc7b0fb35f9c0")]
public class OverpenetrationModifierInitiator : OverpenetrationModifier, IInitiatorRulebookHandler<RuleCalculateStatsWeapon>, IRulebookHandler<RuleCalculateStatsWeapon>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, IHashable
{
	public void OnEventAboutToTrigger(RuleCalculateStatsWeapon rule)
	{
		Apply(rule);
	}

	public void OnEventDidTrigger(RuleCalculateStatsWeapon evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateDamage rule)
	{
	}

	public void OnEventDidTrigger(RuleCalculateDamage rule)
	{
		ApplyOverpenIgnoreDecreament(rule);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
