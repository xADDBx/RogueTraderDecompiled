using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("186465aada0f422b966541bbf050c271")]
public class WarhammerArmorBonus : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateStatsArmor>, IRulebookHandler<RuleCalculateStatsArmor>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ContextValueModifierWithType BonusDeflectionValue;

	public ContextValueModifierWithType BonusAbsorptionValue;

	public bool ForceDeflectionMinimum;

	[ShowIf("ForceDeflectionMinimum")]
	public int PctDeflectionMinimum;

	[ShowIf("ForceDeflectionMinimum")]
	public int DeflectionMinimumValue;

	public bool ForceAbsorptionMinimum;

	[ShowIf("ForceAbsorptionMinimum")]
	public int PctAbsorptionMinimum;

	[ShowIf("ForceAbsorptionMinimum")]
	public int AbsorptionMinimumValue;

	public ModifierDescriptor ModifierDescriptor;

	public void OnEventAboutToTrigger(RuleCalculateStatsArmor evt)
	{
		if (Restrictions.IsPassed(base.Fact, evt))
		{
			if (ForceDeflectionMinimum)
			{
				evt.MinDeflectionValue = DeflectionMinimumValue;
				evt.PctMinDeflection = PctDeflectionMinimum;
			}
			if (ForceAbsorptionMinimum)
			{
				evt.MinAbsorptionValue = AbsorptionMinimumValue;
				evt.PctMinAbsorption = PctAbsorptionMinimum;
			}
			BonusDeflectionValue.TryApply(evt.DeflectionCompositeModifiers, base.Fact, ModifierDescriptor);
			BonusAbsorptionValue.TryApply(evt.AbsorptionCompositeModifiers, base.Fact, ModifierDescriptor);
		}
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
