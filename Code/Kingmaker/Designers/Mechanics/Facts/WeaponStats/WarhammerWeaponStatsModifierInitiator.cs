using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.WeaponStats;

[Serializable]
[TypeId("28fa403fa7a94a2f9ea361583e66cdb4")]
public class WarhammerWeaponStatsModifierInitiator : WarhammerWeaponStatsModifier, IInitiatorRulebookHandler<RuleCalculateStatsWeapon>, IRulebookHandler<RuleCalculateStatsWeapon>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public void OnEventAboutToTrigger(RuleCalculateStatsWeapon evt)
	{
		TryApply(evt);
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
