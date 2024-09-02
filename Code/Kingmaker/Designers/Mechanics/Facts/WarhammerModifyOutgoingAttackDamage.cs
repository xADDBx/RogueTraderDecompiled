using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("28a78c842799f6b42bb3970a9fd77371")]
public class WarhammerModifyOutgoingAttackDamage : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateRighteousFuryChance>, IRulebookHandler<RuleCalculateRighteousFuryChance>, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ITurnStartHandler, ISubscriber<IMechanicEntity>, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public bool AttackedThisTurn;

		public List<MechanicEntity> EntitiesAttackedThisTurn = new List<MechanicEntity>();
	}

	public ContextValue AdditionalDamageMin;

	public ContextValue AdditionalDamageMax;

	public ContextValue AdditionalArmorPenetration;

	public ContextValue AdditionalAbsorption;

	public ContextValue AdditionalDeflection;

	public ContextValue AdditionalRighteousFuryChances;

	public bool OnlyFirstAttack;

	public bool OnlyFirstAttackAgainstEveryTarget;

	public bool OnlyAgainstCaster;

	public bool OnlyAgainstPriorityTarget;

	public bool ActionsOnlyOnMelee;

	public bool ActionsOnlyOnFirstAttack;

	public bool DoNotUseOnDOTs;

	public ActionList ActionsOnAttack;

	[SerializeField]
	[ShowIf("OnlyAgainstPriorityTarget")]
	private BlueprintBuffReference m_TargetBuff;

	public float Multiplier = 1f;

	public bool SpecificRangeType;

	[ShowIf("SpecificRangeType")]
	public WeaponRangeType WeaponRangeType;

	public bool SpecificWeaponFamily;

	[ShowIf("SpecificWeaponFamily")]
	public WeaponFamily WeaponFamily = WeaponFamily.Bolt;

	public bool OnlyChosenWeapon;

	public BlueprintBuff TargetBuff => m_TargetBuff?.Get();

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (CheckConditions(evt, evt.Ability))
		{
			float multiplier = Multiplier;
			float num = ((Multiplier < 0f) ? (-0.5f) : 0.5f);
			evt.MinValueModifiers.Add((int)((float)AdditionalDamageMin.Calculate(base.Context) * multiplier + num), base.Fact);
			evt.MaxValueModifiers.Add((int)((float)AdditionalDamageMax.Calculate(base.Context) * multiplier + num), base.Fact);
			evt.Penetration.Add(ModifierType.ValAdd, (int)((float)AdditionalArmorPenetration.Calculate(base.Context) * multiplier + num), base.Fact);
			evt.Absorption.Add(ModifierType.ValAdd, (int)((float)AdditionalAbsorption.Calculate(base.Context) * multiplier + num), base.Fact);
			evt.Deflection.Add(ModifierType.ValAdd, (int)((float)AdditionalDeflection.Calculate(base.Context) * multiplier + num), base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateRighteousFuryChance evt)
	{
		if (CheckConditions(evt, evt.Ability))
		{
			float multiplier = Multiplier;
			float num = ((Multiplier < 0f) ? (-0.5f) : 0.5f);
			evt.ChanceModifiers.Add((int)((float)AdditionalRighteousFuryChances.Calculate(base.Context) * multiplier + num), base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateRighteousFuryChance evt)
	{
	}

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		TryRunActions(evt);
	}

	private void TryRunActions(RuleDealDamage evt)
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (!CheckConditions(evt, evt.SourceAbility))
		{
			return;
		}
		bool flag = evt.SourceAbility?.Weapon != null && evt.SourceAbility.Weapon.Blueprint.IsMelee;
		if ((!ActionsOnlyOnMelee || flag) && (!ActionsOnlyOnFirstAttack || !componentData.AttackedThisTurn))
		{
			using (base.Context.GetDataScope(evt.ConcreteTarget.ToITargetWrapper()))
			{
				ActionsOnAttack.Run();
			}
			componentData.AttackedThisTurn = true;
			componentData.EntitiesAttackedThisTurn.Add(evt.ConcreteTarget);
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		MechanicEntity mechanicEntity = base.Context?.MaybeCaster;
		MechanicEntity mechanicEntity2 = EventInvokerExtensions.MechanicEntity;
		if (mechanicEntity != null && mechanicEntity2 == mechanicEntity)
		{
			ComponentData componentData = RequestTransientData<ComponentData>();
			componentData.AttackedThisTurn = false;
			componentData.EntitiesAttackedThisTurn.Clear();
		}
	}

	public bool CheckConditions(RulebookEvent evt, AbilityData ability)
	{
		if (DoNotUseOnDOTs && ability == null)
		{
			return false;
		}
		if (ability != null)
		{
			if (base.OwnerBlueprint is BlueprintAbility && ability.Blueprint != base.OwnerBlueprint)
			{
				return false;
			}
			if (SpecificRangeType && ability.Weapon != null && !WeaponRangeType.IsSuitableWeapon(ability.Weapon))
			{
				return false;
			}
			if (SpecificWeaponFamily && ability.Weapon?.Blueprint?.Family != WeaponFamily)
			{
				return false;
			}
			if (OnlyChosenWeapon && (ability.Weapon == null || ability.Weapon != base.Owner.GetOptional<WarhammerUnitPartChooseWeapon>()?.ChosenWeapon))
			{
				return false;
			}
		}
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (OnlyFirstAttack && componentData.AttackedThisTurn)
		{
			return false;
		}
		MechanicEntity mechanicEntity = base.Context?.MaybeCaster;
		if (OnlyAgainstCaster && mechanicEntity != evt.GetRuleTarget())
		{
			return false;
		}
		if (OnlyFirstAttackAgainstEveryTarget && componentData.EntitiesAttackedThisTurn.Contains((MechanicEntity)evt.GetRuleTarget()))
		{
			return false;
		}
		if (OnlyAgainstPriorityTarget)
		{
			BaseUnitEntity baseUnitEntity = base.Context?.MaybeCaster?.GetOptional<UnitPartPriorityTarget>()?.GetPriorityTarget(TargetBuff);
			if (baseUnitEntity == null || baseUnitEntity != evt.GetRuleTarget())
			{
				return false;
			}
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
