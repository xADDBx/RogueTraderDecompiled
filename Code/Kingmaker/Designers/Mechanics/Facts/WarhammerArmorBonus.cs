using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("186465aada0f422b966541bbf050c271")]
public class WarhammerArmorBonus : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateStatsArmor>, IRulebookHandler<RuleCalculateStatsArmor>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public ContextValueModifierWithType BonusDeflectionValue;

	public ContextValueModifierWithType BonusAbsorptionValue;

	public ModifierDescriptor ModifierDescriptor;

	public void OnEventAboutToTrigger(RuleCalculateStatsArmor evt)
	{
		BonusDeflectionValue.TryApply(evt.DeflectionCompositeModifiers, base.Fact, ModifierDescriptor);
		BonusAbsorptionValue.TryApply(evt.AbsorptionCompositeModifiers, base.Fact, ModifierDescriptor);
	}

	public void OnEventDidTrigger(RuleCalculateStatsArmor evt)
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
