using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("a486c202a5e79a04f8c58e75382c2a46")]
public class AbilityDeliverAttackWithWeapon : AbilityDeliverEffect
{
	private AbilityDeliveryTarget TriggerAttackRule(AbilityExecutionContext context, MechanicEntity target)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			PFLog.Default.Error(this, "Caster is missing");
			return null;
		}
		ItemEntityWeapon weapon = context.Ability.Weapon;
		RulePerformAttack rulePerformAttack = new RulePerformAttack(maybeCaster, target, context.Ability, 0);
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

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		if (!IsMelee(context))
		{
			return DeliverShot(context, target);
		}
		return DeliverStrike(context, target);
	}

	private AbilityProjectileAttack DeliverShot(AbilityExecutionContext context, TargetWrapper target)
	{
		if (context.Ability.BurstAttacksCount <= 1)
		{
			return AbilityProjectileAttack.CreateSingleTarget(context, target.Entity, 1);
		}
		return AbilityProjectileAttack.CreateScatter(context, target, context.Ability.BurstAttacksCount, controlledScatter: false);
	}

	private IEnumerator<AbilityDeliveryTarget> DeliverStrike(AbilityExecutionContext context, TargetWrapper target)
	{
		for (int i = 0; i < context.Ability.BurstAttacksCount; i++)
		{
			yield return TriggerAttackRule(context, target.Entity);
		}
	}

	private bool IsMelee(AbilityExecutionContext context)
	{
		return context.Ability.Weapon.Blueprint.Range == WeaponRange.Melee;
	}
}
