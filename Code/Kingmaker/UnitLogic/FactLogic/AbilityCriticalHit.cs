using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("7db3b46096964e54bebd90deeee5235b")]
public class AbilityCriticalHit : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierDescriptor ModifierDescriptor;

	[SerializeField]
	private BlueprintAbilityReference m_BaseAbility;

	[SerializeField]
	private BlueprintMechanicEntityFact.Reference[] m_RestrictionIgnoringCriticalSources;

	public BlueprintAbility BaseAbility => m_BaseAbility;

	public ReferenceArrayProxy<BlueprintMechanicEntityFact> RestrictionIgnoringCriticalSources
	{
		get
		{
			BlueprintReference<BlueprintMechanicEntityFact>[] restrictionIgnoringCriticalSources = m_RestrictionIgnoringCriticalSources;
			return restrictionIgnoringCriticalSources;
		}
	}

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
		AbilityData abilityData = evt.SourceAbility;
		BlueprintScriptableObject blueprintScriptableObject = evt.Reason.Context?.AssociatedBlueprint;
		if (abilityData == null && blueprintScriptableObject == null)
		{
			return;
		}
		bool flag = false;
		if (RestrictionIgnoringCriticalSources.HasReference(blueprintScriptableObject))
		{
			abilityData = base.Owner.Abilities.Enumerable.FindOrDefault((Ability p) => p.Blueprint == BaseAbility)?.Data;
			flag = true;
		}
		if (abilityData == null || (!Restrictions.IsPassed(base.Fact, evt, evt.SourceAbility) && !flag))
		{
			return;
		}
		int bonusCriticalChance = Rulebook.Trigger(new RuleCalculateRighteousFuryChance(base.Owner, evt.TargetUnit, abilityData)).BonusCriticalChance;
		if (Rulebook.Trigger(new RuleRollD100(base.Owner)).Result <= bonusCriticalChance)
		{
			int value = Rulebook.Trigger(new RuleCalculateDamage(base.Owner, evt.TargetUnit, abilityData)
			{
				FakeRule = true
			}).CriticalDamageModifiers.AllModifiersList.Sum((Modifier p) => p.Value) + 50;
			evt.Damage.Modifiers.Add(ModifierType.PctAdd, value, base.Fact, ModifierDescriptor);
			EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IFakeCriticalHandler>)delegate(IFakeCriticalHandler h)
			{
				h.HandleFakeCritical(evt);
			}, isCheckRuntime: true);
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
