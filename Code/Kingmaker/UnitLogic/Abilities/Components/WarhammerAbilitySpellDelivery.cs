using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[TypeId("512b5e789c9c4aa887a5a51d828ba555")]
public class WarhammerAbilitySpellDelivery : AbilityCustomLogic
{
	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		ProjectileLauncher projectileLauncher = new ProjectileLauncher(context.Ability.ProjectileVariants.Random(PFStatefulRandom.UnitLogic.Abilities), context.Caster, target).Ability(context.Ability);
		if (context.CastPosition != context.Caster.Position)
		{
			projectileLauncher = projectileLauncher.LaunchPosition(context.CastPosition);
		}
		Projectile projectile = projectileLauncher.Launch();
		float distance = projectile.Distance(target.Point, context.Caster.Position);
		while (!projectile.IsEnoughTimePassedToTraverseDistance(distance))
		{
			yield return null;
		}
		yield return new AbilityDeliveryTarget(target)
		{
			Projectile = projectile
		};
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
	}
}
