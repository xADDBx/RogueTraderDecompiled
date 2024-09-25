using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("9aad5b8b341240698d08ada2b0175605")]
public sealed class MultiProjectileDistribution : CustomProjectileDistribution
{
	[SerializeField]
	private ContextValue m_MinProjectilesCountPerTarget;

	[SerializeField]
	private ContextValue m_MaxProjectilesCountPerTarget;

	public override List<Projectile> Launch(AbilityExecutionContext context, BlueprintProjectile blueprintProjectile, IEnumerable<MechanicEntity> targets)
	{
		int minProjectilesCountPerTarget = m_MinProjectilesCountPerTarget.Calculate(context);
		int maxProjectilesCountPerTarget = m_MaxProjectilesCountPerTarget.Calculate(context);
		MultiProjectileLauncher multiProjectileLauncher = new MultiProjectileLauncher(context, blueprintProjectile, minProjectilesCountPerTarget, maxProjectilesCountPerTarget);
		multiProjectileLauncher.Add(targets);
		multiProjectileLauncher.Launch();
		return multiProjectileLauncher.LaunchedProjectiles;
	}
}
