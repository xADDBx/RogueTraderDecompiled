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
[AllowMultipleComponents]
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

	[Space(4f)]
	public bool ForcePctAbsorptionMaximum;

	[ShowIf("ForcePctAbsorptionMaximum")]
	public ContextValue PctAbsorptionMaximum;

	public bool ForceAbsorptionMaximum;

	[ShowIf("ForceAbsorptionMaximum")]
	public ContextValue AbsorptionMaximumValue;

	[Space(4f)]
	public bool ForcePctDeflectionMaximum;

	[ShowIf("ForcePctDeflectionMaximum")]
	public ContextValue PctDeflectionMaximum;

	public bool ForceDeflectionMaximum;

	[ShowIf("ForceDeflectionMaximum")]
	public ContextValue DeflectionMaximumValue;

	public ModifierDescriptor ModifierDescriptor;

	public void OnEventAboutToTrigger(RuleCalculateStatsArmor evt)
	{
		if (Restrictions.IsPassed(base.Fact, evt))
		{
			if (ForceDeflectionMinimum)
			{
				evt.StatLimits.MinDeflectionFlat = new RuleCalculateStatsArmor.LimitEntry(base.Fact, DeflectionMinimumValue);
				evt.StatLimits.MinDeflectionPct = new RuleCalculateStatsArmor.LimitEntry(base.Fact, PctDeflectionMinimum);
			}
			if (ForceAbsorptionMinimum)
			{
				evt.StatLimits.MinAbsorptionFlat = new RuleCalculateStatsArmor.LimitEntry(base.Fact, AbsorptionMinimumValue);
				evt.StatLimits.MinAbsorptionPct = new RuleCalculateStatsArmor.LimitEntry(base.Fact, PctAbsorptionMinimum);
			}
			if (ForceDeflectionMaximum)
			{
				evt.StatLimits.MaxDeflectionFlat = new RuleCalculateStatsArmor.LimitEntry(base.Fact, DeflectionMaximumValue.Calculate(base.Context));
			}
			if (ForcePctDeflectionMaximum)
			{
				evt.StatLimits.MaxDeflectionPct = new RuleCalculateStatsArmor.LimitEntry(base.Fact, PctDeflectionMaximum.Calculate(base.Context));
			}
			if (ForceAbsorptionMaximum)
			{
				evt.StatLimits.MaxAbsorptionFlat = new RuleCalculateStatsArmor.LimitEntry(base.Fact, AbsorptionMaximumValue.Calculate(base.Context));
			}
			if (ForcePctAbsorptionMaximum)
			{
				evt.StatLimits.MaxAbsorptionPct = new RuleCalculateStatsArmor.LimitEntry(base.Fact, PctAbsorptionMaximum.Calculate(base.Context));
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
