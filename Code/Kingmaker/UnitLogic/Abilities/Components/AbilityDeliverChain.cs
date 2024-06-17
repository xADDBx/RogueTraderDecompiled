using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("8779bb9fcaf367842a53834d9e956006")]
public class AbilityDeliverChain : AbilityDeliverEffect
{
	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("ProjectileFirst")]
	private BlueprintProjectileReference m_ProjectileFirst;

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("Projectile")]
	private BlueprintProjectileReference m_Projectile;

	public ContextValue TargetsCount;

	public int Radius;

	public bool TargetDead;

	[SerializeField]
	private TargetType m_TargetType;

	[SerializeField]
	private ConditionsChecker m_Condition;

	public BlueprintProjectile ProjectileFirst => m_ProjectileFirst?.Get();

	public BlueprintProjectile Projectile => m_Projectile?.Get();

	public TargetType TargetType => m_TargetType;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		MechanicEntity currentLauncher = context.MaybeCaster;
		MechanicEntity currentTarget = target.Entity;
		if (currentLauncher == null)
		{
			PFLog.Default.Error(this, "Caster is missing");
			yield break;
		}
		if (currentTarget == null)
		{
			PFLog.Default.Error("Can't use chain delivery for points");
			yield break;
		}
		HashSet<MechanicEntity> usedTargets = new HashSet<MechanicEntity> { currentTarget };
		int targetsCount = TargetsCount.Calculate(context);
		int i = 0;
		while (i < targetsCount)
		{
			IEnumerator<TargetWrapper> delivery = DeliverInternal(context, currentLauncher, currentTarget, i == 0);
			while (delivery.MoveNext())
			{
				if (delivery.Current != null)
				{
					yield return new AbilityDeliveryTarget(delivery.Current);
				}
				else
				{
					yield return null;
				}
			}
			int num = i + 1;
			i = num;
			yield return null;
			if (i < targetsCount)
			{
				currentLauncher = currentTarget;
				currentTarget = SelectNextTarget(context, target, usedTargets);
				if (currentTarget == null)
				{
					break;
				}
				usedTargets.Add(currentTarget);
			}
		}
	}

	private IEnumerator<TargetWrapper> DeliverInternal(AbilityExecutionContext context, MechanicEntity launcher, MechanicEntity target, bool isFirst)
	{
		BlueprintProjectile blueprint = (isFirst ? ProjectileFirst : Projectile);
		Projectile proj = new ProjectileLauncher(blueprint, launcher, target).Ability(context.Ability).Launch();
		float distance = proj.Distance(launcher.Position, target.Position);
		while (!proj.IsEnoughTimePassedToTraverseDistance(distance))
		{
			if (proj.Cleared)
			{
				yield break;
			}
			yield return null;
		}
		yield return target;
	}

	private BaseUnitEntity SelectNextTarget(AbilityExecutionContext context, TargetWrapper originalTarget, HashSet<MechanicEntity> usedTargets)
	{
		Vector3 point = originalTarget.Point;
		float num = float.MaxValue;
		BaseUnitEntity result = null;
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.State.AllBaseUnits)
		{
			if (context.Ability.IsValidTargetForAttack(allBaseUnit) && allBaseUnit.IsInCombat)
			{
				float num2 = allBaseUnit.DistanceToInCells(point);
				if (CheckTarget(context, allBaseUnit) && num2 <= (float)Radius && !usedTargets.Contains(allBaseUnit) && num2 < num)
				{
					num = num2;
					result = allBaseUnit;
				}
			}
		}
		return result;
	}

	private bool CheckTarget(AbilityExecutionContext context, BaseUnitEntity unit)
	{
		if (context.MaybeCaster == null)
		{
			PFLog.Default.Error(this, "Caster is missing");
			return false;
		}
		if (unit.LifeState.IsDead && !TargetDead)
		{
			return false;
		}
		if ((m_TargetType == TargetType.Enemy && !context.MaybeCaster.IsEnemy(unit)) || (m_TargetType == TargetType.Ally && context.MaybeCaster.IsEnemy(unit)))
		{
			return false;
		}
		if (m_Condition.HasConditions)
		{
			using (context.GetDataScope(unit.ToITargetWrapper()))
			{
				return m_Condition.Check();
			}
		}
		return true;
	}
}
