using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;

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
		int burstAttacksCount = context.Ability.BurstAttacksCount;
		List<IEnumerator<AbilityDeliveryTarget>> projectileProcesses = (from i in Enumerable.Range(0, burstAttacksCount)
			select DeliverStrike(context, target, i)).ToList();
		while (projectileProcesses.HasItem((IEnumerator<AbilityDeliveryTarget> i) => i != null))
		{
			int i = 0;
			while (i < Math.Min(context.ActionIndex, projectileProcesses.Count))
			{
				IEnumerator<AbilityDeliveryTarget> line = projectileProcesses[i];
				if (line != null)
				{
					bool flag;
					while ((flag = line.MoveNext()) && line.Current != null)
					{
						yield return line.Current;
					}
					if (!flag)
					{
						projectileProcesses[i] = null;
					}
				}
				int num = i + 1;
				i = num;
			}
			yield return null;
		}
	}

	private IEnumerator<AbilityDeliveryTarget> DeliverStrike(AbilityExecutionContext context, TargetWrapper target, int index)
	{
		yield return TriggerAttackRule(context, target.Entity);
	}
}
