using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[TypeId("48bc013150074ae8b49ce84959124bd3")]
public class WarhammerContextActionPerformAttack : ContextAction
{
	public bool UseCurrentWeapon;

	[ShowIf("UseCurrentWeapon")]
	public bool OnlyMeleeWeapon;

	public bool PerformActionsOnDamagePortion;

	public bool PerformActionsOnHit;

	public bool PerformActionsOnKill;

	[ShowIf("PerformActionsOnDamagePortion")]
	public int PercentOfMaxDamageNeededForActions;

	[ShowIf("PerformActionsOnDamagePortion")]
	public ActionList ActionsOnDamagePortion;

	[ShowIf("PerformActionsOnKill")]
	public ActionList ActionsOnKill;

	[ShowIf("PerformActionsOnHit")]
	public ActionList ActionsOnHit;

	public bool PerformWeaponSpecificOnHitActions;

	public bool UseSpecificWeaponClassification;

	[ShowIf("UseSpecificWeaponClassification")]
	public WeaponClassification Classification;

	protected override void RunAction()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		MechanicEntity entity = base.Target.Entity;
		if (maybeCaster == null || entity == null)
		{
			return;
		}
		AbilityData abilityData = base.Context.SourceAbilityContext?.Ability;
		ItemEntityWeapon itemEntityWeapon = (UseCurrentWeapon ? maybeCaster.GetFirstWeapon() : abilityData?.Weapon);
		ItemEntityWeapon itemEntityWeapon2 = (UseCurrentWeapon ? maybeCaster.GetSecondWeapon() : abilityData?.Weapon);
		ItemEntityWeapon itemEntityWeapon3 = ((itemEntityWeapon != null && (!OnlyMeleeWeapon || itemEntityWeapon.Blueprint.IsMelee) && (!UseSpecificWeaponClassification || itemEntityWeapon.Blueprint.Classification == Classification)) ? itemEntityWeapon : itemEntityWeapon2);
		if (UseCurrentWeapon)
		{
			BlueprintAbility blueprintAbility = itemEntityWeapon3?.Blueprint.WeaponAbilities.FirstOrDefault()?.Ability;
			if (blueprintAbility == null)
			{
				return;
			}
			abilityData = new AbilityData(blueprintAbility, maybeCaster)
			{
				IsCharge = (abilityData?.IsCharge ?? false),
				OverrideWeapon = itemEntityWeapon3
			};
		}
		if (abilityData == null || itemEntityWeapon3 == null || (OnlyMeleeWeapon && !itemEntityWeapon3.Blueprint.IsMelee))
		{
			return;
		}
		RulePerformAttack rulePerformAttack = new RulePerformAttack(maybeCaster, entity, abilityData, 0);
		if (base.AbilityContext != null)
		{
			rulePerformAttack.RollPerformAttackRule.DangerArea.UnionWith(base.AbilityContext.Pattern.Nodes);
		}
		Rulebook.Trigger(rulePerformAttack);
		if (rulePerformAttack.ResultIsHit)
		{
			if (PerformActionsOnHit)
			{
				using (base.Context.GetDataScope((TargetWrapper)entity))
				{
					ActionsOnHit.Run();
				}
			}
			if (PerformWeaponSpecificOnHitActions)
			{
				WeaponAbility weaponAbility = ((!UseCurrentWeapon) ? itemEntityWeapon?.Blueprint.WeaponAbilities.FirstOrDefault() : itemEntityWeapon3?.Blueprint.WeaponAbilities.FirstOrDefault());
				if (weaponAbility != null)
				{
					if (weaponAbility.OnHitActions != null)
					{
						using (base.Context.GetDataScope(entity))
						{
							weaponAbility.OnHitActions?.OnHitActions.Run();
						}
					}
					AbilityEffectRunAction component = weaponAbility.Ability.GetComponent<AbilityEffectRunAction>();
					if (component != null)
					{
						using (base.Context.GetDataScope(entity))
						{
							component.Actions.Run();
						}
					}
				}
			}
		}
		RuleDealDamage resultDamageRule = rulePerformAttack.ResultDamageRule;
		if (resultDamageRule == null || resultDamageRule.Result * 100 < itemEntityWeapon3.Blueprint.WarhammerMaxDamage * PercentOfMaxDamageNeededForActions)
		{
			return;
		}
		if (PerformActionsOnDamagePortion)
		{
			using (base.Context.GetDataScope((TargetWrapper)entity))
			{
				ActionsOnDamagePortion.Run();
			}
		}
		if (!PerformActionsOnKill)
		{
			return;
		}
		PartHealth targetHealth = resultDamageRule.TargetHealth;
		if (targetHealth == null || targetHealth.HitPointsLeft > 0 || resultDamageRule.HPBeforeDamage <= 0)
		{
			return;
		}
		using (base.Context.GetDataScope((TargetWrapper)entity))
		{
			ActionsOnKill.Run();
		}
	}

	public override string GetCaption()
	{
		if (!UseCurrentWeapon)
		{
			return "Perform an attack with the weapon that gave this ability";
		}
		return "Perform an attack with the current weapon";
	}
}
