using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Projectiles;

public class MultiProjectileLauncher
{
	private readonly AbilityExecutionContext m_Context;

	private readonly BlueprintProjectile m_Blueprint;

	private readonly int m_MinProjectilesCountPerTarget;

	private readonly int m_MaxProjectilesCountPerTarget;

	private readonly List<MechanicEntity> m_Targets;

	public readonly List<Projectile> LaunchedProjectiles;

	public MultiProjectileLauncher([NotNull] AbilityExecutionContext context, [NotNull] BlueprintProjectile blueprint, int minProjectilesCountPerTarget, int maxProjectilesCountPerTarget)
	{
		m_Context = context;
		m_Blueprint = blueprint;
		m_MinProjectilesCountPerTarget = minProjectilesCountPerTarget;
		m_MaxProjectilesCountPerTarget = maxProjectilesCountPerTarget;
		m_Targets = new List<MechanicEntity>();
		LaunchedProjectiles = new List<Projectile>();
	}

	public void Add(MechanicEntity target)
	{
		m_Targets.Add(target);
	}

	public void Add(IEnumerable<MechanicEntity> targets)
	{
		m_Targets.AddRange(targets);
	}

	public void Launch()
	{
		int projectilesCount = ((m_Targets.Count > 1) ? m_MinProjectilesCountPerTarget : m_MaxProjectilesCountPerTarget);
		foreach (MechanicEntity target in m_Targets)
		{
			Launch(m_Context, target, m_Blueprint, projectilesCount);
		}
	}

	private void Launch([NotNull] AbilityExecutionContext context, [NotNull] TargetWrapper target, [NotNull] BlueprintProjectile projectileBlueprint, int projectilesCount)
	{
		Vector3 position = context.Caster.Position;
		bool flag = false;
		while (projectilesCount > 0)
		{
			TargetWrapper launcher = new TargetWrapper(position, null, context.Caster);
			Projectile item = new ProjectileLauncher(projectileBlueprint, launcher, target).Ability(context.Ability).MaxRangeCells(context.Ability.RangeCells).MisdirectionOffset(Vector3.zero)
				.IsCoverHit(isCoverHit: false)
				.SetDoNotPlayHitEffect(flag)
				.SetDoNotDeliverHit(flag)
				.Launch();
			LaunchedProjectiles.Add(item);
			projectilesCount--;
			flag = true;
		}
	}
}
