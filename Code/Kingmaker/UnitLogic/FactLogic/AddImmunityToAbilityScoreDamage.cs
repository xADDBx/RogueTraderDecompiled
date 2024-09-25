using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("d756fbdf6d2d4796b9d953fbd4f8047a")]
public class AddImmunityToAbilityScoreDamage : UnitFactComponentDelegate, ITargetRulebookHandler<RuleDealStatDamage>, IRulebookHandler<RuleDealStatDamage>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public bool Drain;

	[ValidateNoNullEntries]
	public StatType[] StatTypes;

	public void OnEventAboutToTrigger(RuleDealStatDamage evt)
	{
		evt.Immune = (!evt.IsDrain || Drain) && (StatTypes.Length == 0 || Array.IndexOf(StatTypes, evt.Stat.Type) >= 0);
	}

	public void OnEventDidTrigger(RuleDealStatDamage evt)
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
