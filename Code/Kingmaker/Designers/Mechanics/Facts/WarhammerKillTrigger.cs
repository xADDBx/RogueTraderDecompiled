using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("6d519a34c3e747bfa7d858a3c9a4b978")]
public class WarhammerKillTrigger : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public enum PropertyParameter
	{
		None,
		EnemyDifficulty,
		Damage,
		DamageOverflow,
		Penetration,
		EquipmentSlotNumber
	}

	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	public ActionList ActionsOnKill;

	public ActionList ActionsOnSurvive;

	public bool ActionsOnTarget;

	public bool RefundActionPointsOnKill;

	public bool RefundActionPointsOnSurvive;

	public bool ResetCooldownOnKill;

	public bool ResetCooldownOnSurvive;

	public bool RemoveOnKill;

	public bool RemoveOnSurvive;

	public bool OnlyEnemyKill;

	[SerializeField]
	[Tooltip("Основная группа, может отсутствовать")]
	private BlueprintAbilityGroupReference m_AffectedGroup;

	[SerializeField]
	[Tooltip("Дополнительные группы, приоритет как у основной")]
	private BlueprintAbilityGroupReference[] m_OtherAffectedGroupList;

	[SerializeField]
	private BlueprintUnitFactReference[] m_FilterFacts = new BlueprintUnitFactReference[0];

	public ContextPropertyName ContextPropertyName;

	public PropertyParameter PropertyToSave;

	private bool HasAffectedGroups
	{
		get
		{
			if (m_AffectedGroup == null)
			{
				BlueprintAbilityGroupReference[] otherAffectedGroupList = m_OtherAffectedGroupList;
				if (otherAffectedGroupList != null)
				{
					return otherAffectedGroupList.Length > 0;
				}
				return false;
			}
			return true;
		}
	}

	public ReferenceArrayProxy<BlueprintUnitFact> FilterFacts
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] filterFacts = m_FilterFacts;
			return filterFacts;
		}
	}

	private IEnumerable<BlueprintAbilityGroup> GetAffectedGroups()
	{
		if (m_AffectedGroup != null)
		{
			yield return m_AffectedGroup.Get();
		}
		if (m_OtherAffectedGroupList == null)
		{
			yield break;
		}
		BlueprintAbilityGroupReference[] otherAffectedGroupList = m_OtherAffectedGroupList;
		foreach (BlueprintAbilityGroupReference blueprintAbilityGroupReference in otherAffectedGroupList)
		{
			if (blueprintAbilityGroupReference != null)
			{
				yield return blueprintAbilityGroupReference.Get();
			}
		}
	}

	void IRulebookHandler<RuleDealDamage>.OnEventAboutToTrigger(RuleDealDamage rule)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		if (evt.HPBeforeDamage <= 0 || !FilterFacts.ToList().All((BlueprintUnitFact p) => evt.ConcreteTarget.Facts.Contains(p)) || !m_Restrictions.IsPassed(base.Fact, evt, evt.SourceAbility))
		{
			return;
		}
		switch (PropertyToSave)
		{
		case PropertyParameter.EnemyDifficulty:
			base.Context[ContextPropertyName] = (int)(((evt.Target as UnitEntity)?.Blueprint.DifficultyType + 1) ?? UnitDifficultyType.Common);
			break;
		case PropertyParameter.Damage:
			base.Context[ContextPropertyName] = evt.Result;
			break;
		case PropertyParameter.DamageOverflow:
			base.Context[ContextPropertyName] = Math.Max(evt.Result - evt.HPBeforeDamage, 0);
			break;
		case PropertyParameter.Penetration:
			base.Context[ContextPropertyName] = Math.Max(evt.Damage.Penetration.Value, 0);
			break;
		case PropertyParameter.EquipmentSlotNumber:
			base.Context[ContextPropertyName] = GetItemEquipmentSlotNumber(base.Owner.Body.EquipmentSlots, evt.SourceAbility?.Weapon);
			break;
		}
		using (base.Context.GetDataScope(base.Owner.ToITargetWrapper()))
		{
			PartHealth targetHealth = evt.TargetHealth;
			if (targetHealth != null && targetHealth.HitPointsLeft > 0)
			{
				TryRunActionOnSurvive(evt);
			}
			else if (!OnlyEnemyKill || base.Owner.IsEnemy(evt.Target))
			{
				TryRunActionsOnKill(evt);
			}
		}
	}

	private void TryRunActionOnSurvive(RuleDealDamage evt)
	{
		if (ActionsOnSurvive.HasActions)
		{
			base.Fact.RunActionInContext(ActionsOnSurvive, (!ActionsOnTarget) ? base.Owner.ToITargetWrapper() : evt.ConcreteTarget.ToITargetWrapper());
		}
		TryRunAdditionalActions(evt, RefundActionPointsOnSurvive, ResetCooldownOnSurvive, RemoveOnSurvive);
	}

	private void TryRunActionsOnKill(RuleDealDamage evt)
	{
		if (ActionsOnKill.HasActions)
		{
			base.Fact.RunActionInContext(ActionsOnKill, (!ActionsOnTarget) ? base.Owner.ToITargetWrapper() : evt.ConcreteTarget.ToITargetWrapper());
		}
		TryRunAdditionalActions(evt, RefundActionPointsOnKill, ResetCooldownOnKill, RemoveOnKill);
	}

	private void TryRunAdditionalActions(RuleDealDamage evt, bool refundActionPoints, bool resetCooldown, bool remove)
	{
		AbilityData sourceAbility = evt.SourceAbility;
		if (sourceAbility == null)
		{
			return;
		}
		if (refundActionPoints)
		{
			base.Owner.CombatState.GainYellowPoint(sourceAbility.CalculateActionPointCost(), base.Context);
		}
		PartAbilityCooldowns abilityCooldownsOptional = base.Owner.GetAbilityCooldownsOptional();
		if (!resetCooldown || abilityCooldownsOptional == null)
		{
			return;
		}
		foreach (BlueprintAbilityGroup affectedGroup in GetAffectedGroups())
		{
			if (affectedGroup != null && sourceAbility.Blueprint.AbilityGroups.Contains(affectedGroup))
			{
				abilityCooldownsOptional.RemoveGroupCooldown(affectedGroup);
				abilityCooldownsOptional.RemoveAbilityCooldown(sourceAbility.Blueprint);
			}
		}
		if (!HasAffectedGroups)
		{
			foreach (BlueprintAbilityGroup abilityGroup in sourceAbility.Blueprint.AbilityGroups)
			{
				abilityCooldownsOptional.RemoveGroupCooldown(abilityGroup);
				abilityCooldownsOptional.RemoveAbilityCooldown(sourceAbility.Blueprint);
			}
		}
		if (remove)
		{
			base.Owner.Facts.Remove(base.Fact);
		}
	}

	private int GetItemEquipmentSlotNumber(List<ItemSlot> itemSlots, ItemEntity itemEntity)
	{
		if (itemSlots == null || itemEntity == null)
		{
			return -1;
		}
		for (int i = 0; i < itemSlots.Count; i++)
		{
			if (itemSlots[i].MaybeItem == itemEntity)
			{
				return i;
			}
		}
		return -1;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
