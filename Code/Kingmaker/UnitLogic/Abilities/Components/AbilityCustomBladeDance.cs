using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
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
		new List<BaseUnitEntity>();
		List<BaseUnitEntity> possibleNodsWithDirection = Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity p) => !p.Features.IsUntargetable && !p.LifeState.IsDeadOrUnconscious && p.IsInCombat && p != caster && p.InRangeInCells(caster, 1)).ToList();
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
		while (burstCounter < Math.Min(RateOfAttack.Calculate(context), context.Ability.OverrideRateOfFire))
		{
			if (burstCounter + 1 != context.ActionIndex)
			{
				yield return null;
				continue;
			}
			BaseUnitEntity baseUnitEntity = possibleNodsWithDirection.Where((BaseUnitEntity p) => !p.IsDeadOrUnconscious && !p.Features.IsUntargetable).Random(PFStatefulRandom.Mechanics);
			caster.ForceLookAt(baseUnitEntity.Position);
			if (hasTwoMeleeWeapons)
			{
				weapon = ((burstCounter % 2 == 0) ? primaryHandMaybeWeapon : secondaryHandMaybeWeapon);
			}
			context.Ability.OverrideWeapon = weapon;
			yield return TriggerAttackRule(context, baseUnitEntity, burstCounter, weapon);
			burstCounter++;
		}
		context.Ability.OverrideWeapon = null;
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
