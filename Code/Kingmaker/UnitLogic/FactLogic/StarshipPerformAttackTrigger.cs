using Code.GameCore.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("f332e1a348e0aab40924f7a450d7c484")]
public class StarshipPerformAttackTrigger : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleStarshipPerformAttack>, IRulebookHandler<RuleStarshipPerformAttack>, ISubscriber, IInitiatorRulebookSubscriber, ITargetRulebookHandler<RuleStarshipPerformAttack>, ITargetRulebookSubscriber, IHashable
{
	private enum TriggerType
	{
		AsInitiator,
		AsTarget,
		Both
	}

	private enum AEType
	{
		Ignore,
		Require,
		Exclude
	}

	[SerializeField]
	private bool PerformActionsOnHullDamagePortion;

	[SerializeField]
	private bool PerformActionsOnShieldsDamagePortion;

	[SerializeField]
	private bool PerformActionsOnKill;

	[SerializeField]
	private bool PerformActionsOnSurvive;

	[SerializeField]
	[ShowIf("ShouldPerformActionsOnAnyDamage")]
	private int PercentOfMaxDamageNeededForActions;

	[SerializeField]
	private TriggerType triggerType;

	[SerializeField]
	private AEType aeType;

	[Tooltip("Instead of triggering on each attack in burst, trigger will happen on last attack when at last one of the burst attacks passed the checks")]
	[SerializeField]
	private bool AggregateBurst;

	[SerializeField]
	private bool CheckInitiatorFaction;

	[SerializeField]
	[ShowIf("CheckInitiatorFaction")]
	private BlueprintFactionReference m_Faction;

	public bool CheckWeaponBlueprint;

	[SerializeField]
	[ShowIf("CheckWeaponBlueprint")]
	private BlueprintStarshipWeaponReference[] m_WeaponBlueprints;

	[SerializeField]
	private ActionList Actions;

	[SerializeField]
	private ActionList TargetUnitActions;

	[SerializeField]
	[ShowIf("IsAttachedToAbility")]
	private bool TriggerForThisAbilityOnly = true;

	public ReferenceArrayProxy<BlueprintStarshipWeapon> WeaponBlueprints
	{
		get
		{
			BlueprintReference<BlueprintStarshipWeapon>[] weaponBlueprints = m_WeaponBlueprints;
			return weaponBlueprints;
		}
	}

	public BlueprintFaction Faction => m_Faction?.Get();

	private bool ShouldPerformActionsOnAnyDamage
	{
		get
		{
			if (!PerformActionsOnHullDamagePortion)
			{
				return PerformActionsOnShieldsDamagePortion;
			}
			return true;
		}
	}

	private bool HasAnyConditionsToPerformActions
	{
		get
		{
			if (!ShouldPerformActionsOnAnyDamage && !PerformActionsOnKill)
			{
				return PerformActionsOnSurvive;
			}
			return true;
		}
	}

	private bool IsAttachedToAbility => base.OwnerBlueprint is BlueprintAbility;

	private void RunActions(StarshipEntity target)
	{
		using (base.Fact.MaybeContext?.GetDataScope(base.Owner.ToITargetWrapper()))
		{
			base.Fact.RunActionInContext(Actions, base.Owner.ToITargetWrapper());
			base.Fact.RunActionInContext(TargetUnitActions, target.ToITargetWrapper());
		}
	}

	private Buff.Data CreateBuffContextDataIfNecessary()
	{
		if (!(base.Fact is Buff buff))
		{
			return null;
		}
		return ContextData<Buff.Data>.Request().Setup(buff);
	}

	public void OnEventAboutToTrigger(RuleStarshipPerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RuleStarshipPerformAttack evt)
	{
		if ((triggerType == TriggerType.AsInitiator && evt.Initiator != base.Owner) || (triggerType == TriggerType.AsTarget && evt.Target != base.Owner) || (aeType == AEType.Require && !evt.Weapon.IsAEAmmo) || (aeType == AEType.Exclude && evt.Weapon.IsAEAmmo) || (CheckInitiatorFaction && evt.Initiator.Faction.Blueprint != Faction) || (CheckWeaponBlueprint && !WeaponBlueprints.Contains(evt.Weapon.Blueprint)) || (IsAttachedToAbility && TriggerForThisAbilityOnly && evt.Ability.Blueprint != base.OwnerBlueprint))
		{
			return;
		}
		if (AggregateBurst && evt.FirstAttackInBurst != null)
		{
			for (RuleStarshipPerformAttack ruleStarshipPerformAttack = evt.FirstAttackInBurst; ruleStarshipPerformAttack != null; ruleStarshipPerformAttack = ruleStarshipPerformAttack.NextAttackInBurst)
			{
				if ((ruleStarshipPerformAttack != evt && !ruleStarshipPerformAttack.IsTriggered) || ruleStarshipPerformAttack == ruleStarshipPerformAttack.NextAttackInBurst)
				{
					return;
				}
			}
		}
		bool flag = CanPerformActions(evt, ignoreLifeState: false);
		if (AggregateBurst && !flag)
		{
			for (RuleStarshipPerformAttack ruleStarshipPerformAttack2 = evt.FirstAttackInBurst; ruleStarshipPerformAttack2 != null; ruleStarshipPerformAttack2 = ruleStarshipPerformAttack2.NextAttackInBurst)
			{
				if (ruleStarshipPerformAttack2 != evt && CanPerformActions(ruleStarshipPerformAttack2, ignoreLifeState: true))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			RunActions(evt.Target);
		}
	}

	private bool CanPerformActions(RuleStarshipPerformAttack evt, bool ignoreLifeState)
	{
		if (!HasAnyConditionsToPerformActions)
		{
			return true;
		}
		if (!CanPerformActionsOnHullDamage(evt) && !CanPerformActionsOnShieldsDamage(evt))
		{
			if (!ignoreLifeState)
			{
				return CanPerformActionsOnLifestate(evt);
			}
			return false;
		}
		return true;
	}

	private bool CanPerformActionsOnHullDamage(RuleStarshipPerformAttack evt)
	{
		if (PerformActionsOnHullDamagePortion && evt.DamageRule != null && evt.DamageRule.Result > 0)
		{
			return evt.DamageRule.Result * 100 >= evt.Weapon.Ammo.Blueprint.MaxDamage * PercentOfMaxDamageNeededForActions;
		}
		return false;
	}

	private bool CanPerformActionsOnShieldsDamage(RuleStarshipPerformAttack evt)
	{
		if (PerformActionsOnShieldsDamagePortion)
		{
			if (evt.ResultAbsorbedDamage <= 0 || evt.ResultAbsorbedDamage * 100 < evt.Weapon.Ammo.Blueprint.MaxDamage * PercentOfMaxDamageNeededForActions)
			{
				return evt.AttackRollRule.ResultTargetDisruptionMiss;
			}
			return true;
		}
		return false;
	}

	private bool IsKill(RuleStarshipPerformAttack evt)
	{
		if (evt.DamageRule != null)
		{
			PartHealth targetHealth = evt.DamageRule.TargetHealth;
			if (targetHealth != null && targetHealth.HitPointsLeft <= 0)
			{
				return evt.DamageRule.HPBeforeDamage > 0;
			}
		}
		return false;
	}

	private bool CanPerformActionsOnLifestate(RuleStarshipPerformAttack evt)
	{
		if (!PerformActionsOnKill || !IsKill(evt))
		{
			if (PerformActionsOnSurvive)
			{
				return !IsKill(evt);
			}
			return false;
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
