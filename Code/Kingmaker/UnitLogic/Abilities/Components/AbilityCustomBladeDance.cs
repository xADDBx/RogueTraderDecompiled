using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("e4a6ac6448a04df1b10ca34e4577a6d4")]
public class AbilityCustomBladeDance : AbilityCustomLogic
{
	public ContextValue RateOfAttack;

	public bool UseSpecificWeapon;

	[ShowIf("UseSpecificWeapon")]
	public bool UseOnSourceWeapon;

	[ShowIf("CustomSourceWeapon")]
	public bool UseSecondWeapon;

	public bool UseSpecificWeaponClassification;

	[ShowIf("UseSpecificWeaponClassification")]
	public WeaponClassification Classification;

	[UsedImplicitly]
	private bool CustomSourceWeapon
	{
		get
		{
			if (UseOnSourceWeapon)
			{
				return UseSpecificWeapon;
			}
			return false;
		}
	}

	[CanBeNull]
	public MechanicEntity GetFirstTargetToTurnTo(MechanicEntity caster)
	{
		return GetPossibleTargetsList(caster).Random(PFStatefulRandom.Mechanics);
	}

	private static List<MechanicEntity> GetPossibleTargetsList(MechanicEntity caster)
	{
		IEnumerable<BaseUnitEntity> second = Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity p) => CheckUnitTargetable(caster, p));
		return Game.Instance.State.DestructibleEntities.Where((DestructibleEntity p) => CheckDestructibleTargetable(caster, p)).Concat<MechanicEntity>(second).ToList();
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper targetWrapper)
	{
		UnitEntity caster = context.Caster as UnitEntity;
		if (caster == null)
		{
			PFLog.Default.Error("Caster unit is missing");
			yield break;
		}
		if (caster.GetThreatHandMelee() == null)
		{
			PFLog.Default.Error("Invalid caster's weapon");
			yield break;
		}
		List<MechanicEntity> possibleTargets = GetPossibleTargetsList(caster);
		int burstCounter = 0;
		ItemEntityWeapon weapon = context.Ability.Weapon;
		ItemEntityWeapon primaryHandMaybeWeapon = caster.Body.PrimaryHand.MaybeWeapon;
		ItemEntityWeapon secondaryHandMaybeWeapon = caster.Body.SecondaryHand.MaybeWeapon;
		if (UseSpecificWeapon && !UseOnSourceWeapon && !UseSpecificWeaponClassification)
		{
			weapon = (UseSecondWeapon ? (secondaryHandMaybeWeapon ?? primaryHandMaybeWeapon) : (primaryHandMaybeWeapon ?? secondaryHandMaybeWeapon));
		}
		if (UseSpecificWeaponClassification)
		{
			weapon = ((primaryHandMaybeWeapon?.Blueprint.Classification == Classification) ? primaryHandMaybeWeapon : secondaryHandMaybeWeapon);
		}
		bool hasTwoMeleeWeapons = ((!UseSpecificWeaponClassification) ? (!UseSpecificWeapon && primaryHandMaybeWeapon != null && secondaryHandMaybeWeapon != null) : (!UseSpecificWeapon && primaryHandMaybeWeapon?.Blueprint?.Classification == Classification && secondaryHandMaybeWeapon?.Blueprint?.Classification == Classification));
		int rateOfAttack = RateOfAttack.Calculate(context);
		MechanicEntity currentTarget = context.BladeDanceTarget;
		while (burstCounter < Math.Min(rateOfAttack, context.Ability.OverrideRateOfFire) && !(context.MaybeCaster?.IsDeadOrUnconscious ?? false))
		{
			if (currentTarget == null)
			{
				currentTarget = possibleTargets.Where((MechanicEntity p) => CheckEntityTargetable(caster, p)).Random(PFStatefulRandom.Mechanics);
				if (currentTarget == null)
				{
					break;
				}
				context.BladeDanceTarget = currentTarget;
			}
			if (burstCounter + 1 > context.ActionIndex)
			{
				yield return null;
				continue;
			}
			caster.ForceLookAt(currentTarget.Position);
			if (hasTwoMeleeWeapons)
			{
				weapon = ((burstCounter % 2 == 0) ? primaryHandMaybeWeapon : secondaryHandMaybeWeapon);
			}
			context.Ability.OverrideWeapon = weapon;
			AbilityDeliveryTarget abilityDeliveryTarget = TriggerAttackRule(context, currentTarget, burstCounter, weapon);
			WeaponAbility weaponAbility = weapon?.Blueprint.WeaponAbilities.FirstOrDefault();
			if ((abilityDeliveryTarget.AttackRule?.ResultIsHit ?? false) && weaponAbility != null)
			{
				if (weaponAbility.OnHitActions != null)
				{
					using (context.GetDataScope(currentTarget))
					{
						weaponAbility.OnHitActions?.OnHitActions.Run();
					}
				}
				AbilityEffectRunAction component = weaponAbility.Ability.GetComponent<AbilityEffectRunAction>();
				if (component != null)
				{
					using (context.GetDataScope(currentTarget))
					{
						component.Actions.Run();
					}
				}
			}
			yield return abilityDeliveryTarget;
			currentTarget = null;
			burstCounter++;
		}
	}

	public static bool CheckEntityTargetable(MechanicEntity caster, MechanicEntity entity)
	{
		if (!(entity is BaseUnitEntity unit))
		{
			if (entity is DestructibleEntity entity2)
			{
				return CheckDestructibleTargetable(caster, entity2);
			}
			return false;
		}
		return CheckUnitTargetable(caster, unit);
	}

	public static bool CheckUnitTargetable(MechanicEntity caster, BaseUnitEntity unit)
	{
		if (!unit.IsDeadOrUnconscious && !unit.Features.IsUntargetable && unit.IsInCombat && unit != caster)
		{
			return unit.InRangeInCells(caster, 1);
		}
		return false;
	}

	public static bool CheckDestructibleTargetable(MechanicEntity caster, DestructibleEntity entity)
	{
		PartHealth healthOptional = entity.GetHealthOptional();
		if (healthOptional != null && healthOptional.HitPointsLeft > 0 && entity.InRangeInCells(caster, 1))
		{
			return !(entity is CoverEntity);
		}
		return false;
	}

	private AbilityDeliveryTarget TriggerAttackRule(AbilityExecutionContext context, MechanicEntity target, int burstIndex, ItemEntityWeapon weapon)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			PFLog.Default.Error(this, "Caster is missing");
			return null;
		}
		RulePerformAttack rulePerformAttack = new RulePerformAttack(maybeCaster, target, context.Ability, burstIndex);
		context.TriggerRule(rulePerformAttack);
		if (maybeCaster is BaseUnitEntity attacker && target is BaseUnitEntity baseUnitEntity && baseUnitEntity.View != null && baseUnitEntity.View.HitFxManager != null)
		{
			baseUnitEntity.View.HitFxManager.HandleMeleeAttackHit(attacker, AttackResult.Hit, crit: false, weapon);
		}
		return new AbilityDeliveryTarget(target)
		{
			AttackRule = rulePerformAttack
		};
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
	}
}
