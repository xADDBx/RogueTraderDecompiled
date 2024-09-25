using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Enum;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[Obsolete]
[TypeId("76dd00a1f560ad2438ed8bf8cbfcd039")]
[AllowMultipleComponents]
public class AddIncomingDamageTrigger : UnitFactComponentDelegate, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleDealStatDamage>, IRulebookHandler<RuleDealStatDamage>, ITargetRulebookHandler<RuleDrainEnergy>, IRulebookHandler<RuleDrainEnergy>, IHashable
{
	public ActionList Actions;

	public ActionList ActionsToAttacker;

	public bool TriggerOnStatDamageOrEnergyDrain;

	public bool IgnoreDamageFromThisFact;

	public bool ReduceBelowZero;

	public bool CheckDamageDealt;

	[ShowIf("CheckDamageDealt")]
	public CompareOperation.Type CompareType;

	[ShowIf("CheckDamageDealt")]
	public ContextValue TargetValue;

	public bool CheckWeaponAttackType;

	[ShowIf("CheckWeaponAttackType")]
	[EnumFlagsAsButtons]
	public AttackTypeFlag AttackType;

	public bool CheckDamageType;

	[ShowIf("CheckDamageType")]
	public DamageType DamageType;

	public bool TriggersForDamageOverTime;

	private void TryRunAction(RulebookEvent e)
	{
		BlueprintScriptableObject blueprintScriptableObject = e.Reason.Context?.AssociatedBlueprint;
		if (((!(blueprintScriptableObject is BlueprintBuff) && !(blueprintScriptableObject is BlueprintAbilityAreaEffect)) || TriggersForDamageOverTime) && !IgnoreDamageFromThisFact && e.Reason.Fact != base.Fact)
		{
			if (Actions.HasActions)
			{
				base.Fact.RunActionInContext(Actions, base.OwnerTargetWrapper);
			}
			if (ActionsToAttacker.HasActions)
			{
				base.Fact.RunActionInContext(ActionsToAttacker, e.ConcreteInitiator.ToITargetWrapper());
			}
		}
	}

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		if (CheckReduceBelowZero(evt) && CheckDamageValue(evt.Result) && CheckAttackType(evt) && CheckEnergyType(evt))
		{
			TryRunAction(evt);
		}
	}

	public void OnEventAboutToTrigger(RuleDealStatDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealStatDamage evt)
	{
		if (TriggerOnStatDamageOrEnergyDrain && CheckDamageValue(evt.Result))
		{
			TryRunAction(evt);
		}
	}

	public void OnEventAboutToTrigger(RuleDrainEnergy evt)
	{
	}

	public void OnEventDidTrigger(RuleDrainEnergy evt)
	{
		if (TriggerOnStatDamageOrEnergyDrain && CheckDamageValue(evt.DrainValue))
		{
			TryRunAction(evt);
		}
	}

	private bool CheckAttackType(RuleDealDamage evt)
	{
		if (!(evt.Reason.Rule is RulePerformAttack rulePerformAttack))
		{
			return !CheckWeaponAttackType;
		}
		if (CheckWeaponAttackType)
		{
			if (rulePerformAttack.Ability.Weapon != null)
			{
				return AttackType.Contains(rulePerformAttack.Ability.Weapon.Blueprint.AttackType);
			}
			return false;
		}
		return true;
	}

	private bool CheckDamageValue(int damageValue)
	{
		if (CheckDamageDealt)
		{
			return CompareType.CheckCondition(damageValue, TargetValue.Calculate(base.Fact.MaybeContext));
		}
		return true;
	}

	private bool CheckReduceBelowZero(RuleDealDamage evt)
	{
		if (ReduceBelowZero)
		{
			if (evt.TargetHealth != null && evt.TargetHealth.HitPointsLeft <= 0)
			{
				return evt.TargetHealth.HitPointsLeft + evt.Result > 0;
			}
			return false;
		}
		return true;
	}

	private bool CheckEnergyType(RuleDealDamage evt)
	{
		if (CheckDamageType)
		{
			return evt.Damage.Type == DamageType;
		}
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
