using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.WeaponStats;

[Serializable]
[TypeId("de3e4a817a9c4231bae67beded925d80")]
public class WarhammerWeaponStatsReplaceDamage : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateStatsWeapon>, IRulebookHandler<RuleCalculateStatsWeapon>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ContextValueModifier DamageMin;

	public ContextValueModifier DamageMax;

	public void OnEventAboutToTrigger(RuleCalculateStatsWeapon evt)
	{
		if (Restrictions.IsPassed(base.Fact, evt, evt.Ability))
		{
			evt.DamageMinOverride = DamageMin.Calculate(base.Context);
			evt.DamageMaxOverride = DamageMax.Calculate(base.Context);
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
