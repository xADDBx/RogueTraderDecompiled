using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View.Covers;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.PatternAttack;

public class AbilityAoEPatternAttack : IEnumerator<AbilityDeliveryTarget>, IEnumerator, IDisposable
{
	private readonly IEnumerator<AbilityDeliveryTarget> m_Process;

	public AbilityExecutionContext Context { get; }

	public IAbilityAoEPatternProvider Settings { get; }

	public bool IsWeaponAttack { get; }

	public int AdditionalDamageInstancesCount { get; }

	public bool IsFinished { get; private set; }

	public AbilityDeliveryTarget CurrentTarget { get; private set; }

	public bool WeaponAttackDamageDisabled { get; private set; }

	public bool DodgeForAllyDisabled { get; private set; }

	AbilityDeliveryTarget IEnumerator<AbilityDeliveryTarget>.Current => CurrentTarget;

	object IEnumerator.Current => CurrentTarget;

	public AbilityAoEPatternAttack(AbilityExecutionContext context, WarhammerAbilityAttackDelivery attackSettings, IAbilityAoEPatternProvider patternSettings, CustomGridNodeBase fromNode, CustomGridNodeBase toNode)
	{
		Context = context;
		Settings = patternSettings;
		IsWeaponAttack = attackSettings.IsWeaponAttack;
		AdditionalDamageInstancesCount = attackSettings.AdditionalDamageInstancesCount;
		m_Process = Deliver(context, attackSettings, fromNode, toNode);
	}

	private IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, WarhammerAbilityAttackDelivery attackSettings, CustomGridNodeBase fromNode, CustomGridNodeBase toNode)
	{
		BlueprintProjectile blueprintProjectile = context.Ability.ProjectileVariants.Random(PFStatefulRandom.UnitLogic.Abilities);
		if (context.AbilityBlueprint.IsCustomProjectileDistribution)
		{
			return DeliverWithCustomProjectileDistribution(context, blueprintProjectile);
		}
		if (!attackSettings.IsRanged)
		{
			return DeliverDirectly(context);
		}
		if (attackSettings.PatternSpreadWithProjectile)
		{
			return SpreadWithProjectile(context, fromNode, context.Pattern.Nodes.Last(), blueprintProjectile);
		}
		return DeliverWithProjectile(context, fromNode, toNode, blueprintProjectile);
	}

	private AbilityDeliveryTarget DeliverHit(AbilityExecutionContext context, MechanicEntity target, OrientedPatternData pattern, [CanBeNull] Projectile projectile = null, Vector3? effectiveCasterPosition = null)
	{
		if (!IsWeaponAttack)
		{
			return new AbilityDeliveryTarget(target)
			{
				Projectile = projectile
			};
		}
		RulePerformAttack rulePerformAttack = new RulePerformAttack(context.Caster, target, context.Ability, 0, WeaponAttackDamageDisabled, DodgeForAllyDisabled, effectiveCasterPosition)
		{
			AdditionalDamageInstancesCount = AdditionalDamageInstancesCount,
			Reason = context
		};
		rulePerformAttack.RollPerformAttackRule.DangerArea.UnionWith(pattern.Nodes);
		context.TriggerRule(rulePerformAttack);
		return new AbilityDeliveryTarget(target)
		{
			AttackRule = rulePerformAttack,
			Projectile = projectile
		};
	}

	private IEnumerator<AbilityDeliveryTarget> SpreadWithProjectile(AbilityExecutionContext context, CustomGridNodeBase fromNode, CustomGridNodeBase toNode, BlueprintProjectile blueprintProjectile)
	{
		Debug.DrawLine(fromNode.Vector3Position, toNode.Vector3Position, Color.yellow);
		Vector3 castPosition = fromNode.Vector3Position;
		Vector3 vector3Position = toNode.Vector3Position;
		TargetWrapper launcher = new TargetWrapper(castPosition, null, context.Caster);
		Projectile projectile = new ProjectileLauncher(blueprintProjectile, launcher, vector3Position).Ability(context.Ability).MaxRangeCells(context.Ability.RangeCells).Index(0)
			.Launch();
		float distance = projectile.Distance(fromNode.Vector3Position, toNode.Vector3Position);
		bool isIgnoreLevelDifference = Settings.IsIgnoreLevelDifference;
		OrientedPatternData pattern = context.Pattern;
		HashSet<MechanicEntity> usedUnits = new HashSet<MechanicEntity>();
		do
		{
			yield return null;
			Debug.DrawLine(fromNode.Vector3Position, toNode.Vector3Position, Color.yellow);
			if (projectile.Cleared)
			{
				break;
			}
			foreach (MechanicEntity mechanicEntity in Game.Instance.State.MechanicEntities)
			{
				if (!context.Ability.IsValidTargetForAttack(mechanicEntity) || mechanicEntity == context.Caster)
				{
					continue;
				}
				float distance2 = ((!projectile.IsHit) ? projectile.PassedDistance : float.MaxValue);
				if (!AoEPatternHelper.WouldTargetEntityPattern(mechanicEntity, context.Pattern, castPosition, distance2, out var los) || !usedUnits.Add(mechanicEntity))
				{
					continue;
				}
				if ((LosCalculations.CoverType)los != 0 && Rulebook.Trigger(new RuleRollCoverHit(context.Caster, mechanicEntity, context.Ability, los, los.ObstacleEntity)).ResultIsHit)
				{
					if (los.ObstacleEntity != null)
					{
						yield return DeliverHit(context, los.ObstacleEntity, pattern, projectile);
					}
				}
				else
				{
					Debug.DrawLine(mechanicEntity.Position, mechanicEntity.Position + Vector3.up * 3f, Color.red);
					Vector3 value = (isIgnoreLevelDifference ? mechanicEntity.Position : castPosition);
					yield return DeliverHit(context, mechanicEntity, pattern, projectile, value);
				}
			}
		}
		while (!projectile.IsEnoughTimePassedToTraverseDistance(distance));
	}

	private IEnumerator<AbilityDeliveryTarget> DeliverWithProjectile(AbilityExecutionContext context, CustomGridNodeBase fromNode, CustomGridNodeBase toNode, BlueprintProjectile blueprintProjectile)
	{
		Vector3 vector3Position = fromNode.Vector3Position;
		Vector3 vector3Position2 = toNode.Vector3Position;
		TargetWrapper launcher = new TargetWrapper(vector3Position, null, context.Caster);
		Projectile projectile = new ProjectileLauncher(blueprintProjectile, launcher, vector3Position2).Ability(context.Ability).MaxRangeCells(context.Ability.RangeCells).Index(0)
			.Launch();
		float distance = projectile.Distance(fromNode.Vector3Position, toNode.Vector3Position);
		OrientedPatternData pattern = context.Pattern;
		while (true)
		{
			yield return null;
			if (projectile.Cleared)
			{
				break;
			}
			if (!projectile.IsEnoughTimePassedToTraverseDistance(distance))
			{
				continue;
			}
			foreach (MechanicEntity mechanicEntity in Game.Instance.State.MechanicEntities)
			{
				if (!IsValidTarget(context, mechanicEntity))
				{
					continue;
				}
				bool isIgnoreLos = Settings.IsIgnoreLos;
				LosDescription warhammerLos = LosCalculations.GetWarhammerLos(context.Pattern.ApplicationNode.Vector3Position, default(IntRect), mechanicEntity);
				if (!isIgnoreLos && (LosCalculations.CoverType)warhammerLos != 0 && Rulebook.Trigger(new RuleRollCoverHit(context.Caster, mechanicEntity, context.Ability, warhammerLos, warhammerLos.ObstacleEntity)).ResultIsHit)
				{
					if (warhammerLos.ObstacleEntity != null)
					{
						yield return DeliverHit(context, warhammerLos.ObstacleEntity, pattern, projectile);
					}
				}
				else
				{
					yield return DeliverHit(context, mechanicEntity, pattern, projectile);
				}
			}
			break;
		}
	}

	private IEnumerator<AbilityDeliveryTarget> DeliverDirectly(AbilityExecutionContext context)
	{
		OrientedPatternData pattern = context.Pattern;
		foreach (MechanicEntity mechanicEntity in Game.Instance.State.MechanicEntities)
		{
			if (IsValidTarget(context, mechanicEntity, checkTargetType: true))
			{
				yield return DeliverHit(context, mechanicEntity, pattern);
			}
		}
	}

	private IEnumerator<AbilityDeliveryTarget> DeliverWithCustomProjectileDistribution(AbilityExecutionContext context, BlueprintProjectile blueprintProjectile)
	{
		CustomProjectileDistribution component = context.AbilityBlueprint.GetComponent<CustomProjectileDistribution>();
		if (component == null)
		{
			yield break;
		}
		OrientedPatternData pattern = context.Pattern;
		IEnumerable<MechanicEntity> targets = Game.Instance.State.MechanicEntities.Where((MechanicEntity entity) => IsValidTarget(context, entity, checkTargetType: true));
		List<Projectile> projectiles = (from x in component.Launch(context, blueprintProjectile, targets)
			where !x.DoNotDeliverHit
			select x).ToList();
		do
		{
			yield return null;
			for (int index = projectiles.Count - 1; index >= 0; index--)
			{
				Projectile projectile = projectiles[index];
				bool num = projectile.IsEnoughTimePassedToTraverseDistance();
				bool cleared = projectile.Cleared;
				if (num || cleared)
				{
					projectiles.Remove(projectile);
				}
				if (num)
				{
					yield return DeliverHit(context, projectile.Target.Entity, pattern);
				}
			}
		}
		while (projectiles.Count > 0);
	}

	private bool IsValidTarget(AbilityExecutionContext context, MechanicEntity entity, bool checkTargetType = false)
	{
		if (!context.Ability.IsValidTargetForAttack(entity))
		{
			return false;
		}
		if ((context.Ability.IsMelee || context.Ability.IsScatter) && entity == context.Caster)
		{
			return false;
		}
		if (!AoEPatternHelper.WouldTargetEntity(context.Pattern, entity))
		{
			return false;
		}
		if (!checkTargetType)
		{
			return true;
		}
		if (Settings.Targets == TargetType.Any)
		{
			return true;
		}
		PartCombatGroup combatGroupOptional = entity.GetCombatGroupOptional();
		TargetType targets = Settings.Targets;
		if (targets != 0)
		{
			if (targets == TargetType.Ally && (combatGroupOptional == null || !combatGroupOptional.IsAlly(context.Caster)))
			{
				goto IL_009a;
			}
		}
		else if (combatGroupOptional != null && !combatGroupOptional.IsEnemy(context.Caster))
		{
			goto IL_009a;
		}
		return true;
		IL_009a:
		return false;
	}

	public void DisableWeaponAttackDamage()
	{
		WeaponAttackDamageDisabled = true;
	}

	public void DisableDodgeForAlly()
	{
		DodgeForAllyDisabled = true;
	}

	void IEnumerator.Reset()
	{
		throw new NotImplementedException();
	}

	bool IEnumerator.MoveNext()
	{
		if (IsFinished)
		{
			return false;
		}
		IsFinished = !m_Process.MoveNext();
		CurrentTarget = (IsFinished ? null : m_Process.Current);
		return !IsFinished;
	}

	void IDisposable.Dispose()
	{
	}
}
