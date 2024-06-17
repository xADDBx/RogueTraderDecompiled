using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("eae609680122440d89d28229f76d17ff")]
public class WarhammerDamageOverTimeCriticalHit : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierDescriptor ModifierDescriptor;

	[SerializeField]
	private BlueprintAbilityGroupReference m_DamageOverTimeGroup;

	[SerializeField]
	private BlueprintAbilityReference m_BaseAbility;

	public BlueprintAbilityGroup DamageOverTimeGroup => m_DamageOverTimeGroup;

	public BlueprintAbility BaseAbility => m_BaseAbility;

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
		if (!(evt.Reason.Context?.AssociatedBlueprint is BlueprintBuff blueprintBuff) || !blueprintBuff.AbilityGroups.Contains(DamageOverTimeGroup) || evt.TargetUnit == null || !Restrictions.IsPassed(base.Fact, evt, evt.SourceAbility))
		{
			return;
		}
		AbilityData abilityData = base.Owner.Abilities.Enumerable.FindOrDefault((Ability p) => p.Blueprint == BaseAbility)?.Data;
		if (abilityData == null)
		{
			return;
		}
		int bonusCriticalChance = Rulebook.Trigger(new RuleCalculateRighteousFuryChance(base.Owner, evt.TargetUnit, abilityData)).BonusCriticalChance;
		if (Rulebook.Trigger(new RuleRollD100(base.Owner)).Result <= bonusCriticalChance)
		{
			int value = Rulebook.Trigger(new RuleCalculateDamage(base.Owner, evt.TargetUnit, abilityData)).CriticalDamageModifiers.AllModifiersList.Sum((Modifier p) => p.Value) + 50;
			evt.Damage.Modifiers.Add(ModifierType.PctAdd, value, base.Fact, ModifierDescriptor);
		}
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
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
