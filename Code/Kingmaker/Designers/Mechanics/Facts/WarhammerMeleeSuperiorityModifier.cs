using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("fdcb0bfd84d766847a95c3717e443adf")]
public class WarhammerMeleeSuperiorityModifier : UnitBuffComponentDelegate, ITargetRulebookHandler<RuleCalculateSuperiority>, IRulebookHandler<RuleCalculateSuperiority>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public ContextValue SuperiorityModifier;

	public void OnEventAboutToTrigger(RuleCalculateSuperiority evt)
	{
		evt.SuperiorityValueModifiers.Add(SuperiorityModifier.Calculate(base.Context), base.Fact);
	}

	public void OnEventDidTrigger(RuleCalculateSuperiority evt)
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
