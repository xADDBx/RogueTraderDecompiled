using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Buffs;

[ComponentName("BuffMechanics/Ability Roll Bonus")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("e8255e5a137d50245853bf6b21665cdc")]
public class BuffAbilityRollsBonus : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSkillCheck>, IRulebookHandler<RulePerformSkillCheck>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public int Value;

	public ModifierDescriptor Descriptor;

	public bool AffectAllStats;

	public bool OnlyHighesStats;

	public ContextValue Multiplier;

	[ShowIf("ShowStatType")]
	public StatType Stat;

	[UsedImplicitly]
	private bool ShowStatType => !AffectAllStats;

	public void OnEventAboutToTrigger(RulePerformSkillCheck evt)
	{
		int value = Multiplier.Calculate(base.Context) * Value;
		if (!(base.Owner.Stats.GetStat(evt.StatType) is ModifiableValueAttributeStat modifiableValueAttributeStat))
		{
			return;
		}
		if (AffectAllStats || (evt.StatType == Stat && !OnlyHighesStats))
		{
			evt.DifficultyModifiers.Add(value, base.Fact, Descriptor);
			return;
		}
		PartStatsAttributes attributesOptional = evt.Initiator.GetAttributesOptional();
		if (OnlyHighesStats && attributesOptional != null && modifiableValueAttributeStat.ModifiedValue >= (int)attributesOptional.WarhammerWeaponSkill && modifiableValueAttributeStat.ModifiedValue >= (int)attributesOptional.WarhammerBallisticSkill && modifiableValueAttributeStat.ModifiedValue >= (int)attributesOptional.WarhammerStrength && modifiableValueAttributeStat.ModifiedValue >= (int)attributesOptional.WarhammerToughness && modifiableValueAttributeStat.ModifiedValue >= (int)attributesOptional.WarhammerAgility && modifiableValueAttributeStat.ModifiedValue >= (int)attributesOptional.WarhammerPerception && modifiableValueAttributeStat.ModifiedValue >= (int)attributesOptional.WarhammerIntelligence && modifiableValueAttributeStat.ModifiedValue >= (int)attributesOptional.WarhammerWillpower && modifiableValueAttributeStat.ModifiedValue >= (int)attributesOptional.WarhammerFellowship)
		{
			evt.DifficultyModifiers.Add(value, base.Fact, Descriptor);
		}
	}

	public void OnEventDidTrigger(RulePerformSkillCheck evt)
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
