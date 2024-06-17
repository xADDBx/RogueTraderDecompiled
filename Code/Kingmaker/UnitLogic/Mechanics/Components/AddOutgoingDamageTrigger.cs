using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[Obsolete]
[AllowMultipleComponents]
[TypeId("c2129f96be33c7e45917aabea8b92623")]
public class AddOutgoingDamageTrigger : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleDealStatDamage>, IRulebookHandler<RuleDealStatDamage>, IInitiatorRulebookHandler<RuleDrainEnergy>, IRulebookHandler<RuleDrainEnergy>, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public bool WasTargetAlive;
	}

	public ActionList Actions;

	public bool TriggerOnStatDamageOrEnergyDrain;

	public bool CheckAbilityType;

	[ShowIf("CheckAbilityType")]
	public AbilityType m_AbilityType;

	public bool CheckSpellDescriptor;

	public bool CheckSpellParent;

	public bool NotZeroDamage;

	public bool CheckDamageType;

	[ShowIf("CheckDamageType")]
	public DamageType DamageType;

	public bool ApplyToAreaEffectDamage;

	public bool TargetKilledByThisDamage;

	public bool TargetHasFact;

	[ShowIf("CheckSpellParent")]
	[SerializeField]
	private BlueprintAbilityReference[] m_AbilityList;

	[ShowIf("CheckSpellDescriptor")]
	public SpellDescriptorWrapper SpellDescriptorsList;

	public bool OnlyMelee;

	public bool ActionsOnInitiator;

	public bool TriggersForDamageOverTime;

	[SerializeField]
	private BlueprintUnitFactReference[] m_TargetFacts;

	public ReferenceArrayProxy<BlueprintUnitFact> TargetFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] targetFacts = m_TargetFacts;
			return targetFacts;
		}
	}

	public ReferenceArrayProxy<BlueprintAbility> AbilityList
	{
		get
		{
			BlueprintReference<BlueprintAbility>[] abilityList = m_AbilityList;
			return abilityList;
		}
	}

	private void RunAction(MechanicEntity target)
	{
		if (Actions.HasActions)
		{
			base.Fact.RunActionInContext(Actions, target.ToITargetWrapper());
		}
	}

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
		RequestTransientData<ComponentData>().WasTargetAlive = !evt.ConcreteTarget.IsDead;
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		Apply(evt);
	}

	public void OnEventAboutToTrigger(RuleDealStatDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealStatDamage evt)
	{
		if (TriggerOnStatDamageOrEnergyDrain)
		{
			RunAction(evt.Target);
		}
	}

	public void OnEventAboutToTrigger(RuleDrainEnergy evt)
	{
	}

	public void OnEventDidTrigger(RuleDrainEnergy evt)
	{
		if (TriggerOnStatDamageOrEnergyDrain)
		{
			RunAction(evt.Target);
		}
	}

	private void Apply(RuleDealDamage evt)
	{
		RulePerformAttack attack = evt.Reason.Attack;
		if ((CheckAbilityType && evt.Reason.Ability?.Blueprint.Type != m_AbilityType) || (CheckSpellDescriptor && (evt.Reason.Ability == null || !evt.Reason.Ability.Blueprint.SpellDescriptor.HasFlag((SpellDescriptor)SpellDescriptorsList))) || (CheckDamageType && evt.Damage.Type != DamageType) || (OnlyMelee && (attack == null || attack.Ability.Weapon?.Blueprint.IsMelee != true)))
		{
			return;
		}
		BlueprintScriptableObject blueprintScriptableObject = evt.Reason.Context?.AssociatedBlueprint;
		if ((blueprintScriptableObject is BlueprintBuff || blueprintScriptableObject is BlueprintAbilityAreaEffect) && !TriggersForDamageOverTime)
		{
			return;
		}
		if (TargetHasFact)
		{
			bool flag = false;
			foreach (BlueprintUnitFact targetFact in TargetFacts)
			{
				flag |= evt.ConcreteTarget.Facts.Contains(targetFact);
			}
			if (!flag)
			{
				return;
			}
		}
		bool flag2 = evt.Reason.Ability != null && (AbilityList.Contains(evt.Reason.Ability.Blueprint) || AbilityList.Contains(evt.Reason.Ability.Blueprint.Parent));
		bool flag3 = (evt.SourceAbility != null && (AbilityList.Contains(evt.SourceAbility.Blueprint) || AbilityList.Contains(evt.SourceAbility.Blueprint.Parent))) || flag2;
		if ((!ApplyToAreaEffectDamage && (bool)evt.SourceArea) || (CheckSpellParent && !flag3))
		{
			return;
		}
		bool flag4 = evt.TargetHealth != null && (int)evt.TargetHealth.HitPoints <= evt.TargetHealth.Damage;
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (!TargetKilledByThisDamage || (componentData.WasTargetAlive && flag4))
		{
			if (!ActionsOnInitiator)
			{
				RunAction(evt.ConcreteTarget);
			}
			else
			{
				RunAction(evt.ConcreteInitiator);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
