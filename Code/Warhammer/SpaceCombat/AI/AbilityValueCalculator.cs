using System;
using System.Collections.Generic;
using Kingmaker.AI;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Pathfinding;
using UnityEngine;

namespace Warhammer.SpaceCombat.AI;

public class AbilityValueCalculator
{
	private readonly SpaceCombatDecisionContext context;

	private readonly Dictionary<(Ability, MechanicEntity), int> _abilityValueCache = new Dictionary<(Ability, MechanicEntity), int>();

	public AbilityValueCalculator(SpaceCombatDecisionContext context)
	{
		this.context = context;
	}

	public int Calculate(ShipPath.DirectionalPathNode pathNode, Ability ability)
	{
		if (!pathNode.canStand)
		{
			return 0;
		}
		int num = 0;
		BaseUnitEntity unit = context.Unit;
		PartUnitBrain brain = unit.Brain;
		if (!ability.Data.IsAvailable)
		{
			return num;
		}
		List<TargetInfo> targets = context.Enemies;
		(brain?.Blueprint as BlueprintStarshipBrain)?.TryOverrideTargets(context, ref targets);
		int rangeCells = ability.Data.RangeCells;
		IAbilityOverrideCasterForRange component;
		bool flag = ability.Data.Blueprint.GetComponent<AbilityUnrestrictedRangeForTarget>() == null && !ability.Data.Blueprint.TryGetComponent<IAbilityOverrideCasterForRange>(out component);
		IntRect sizeRect = unit.SizeRect;
		Vector3 vector3Direction = CustomGraphHelper.GetVector3Direction(pathNode.direction);
		foreach (TargetInfo item in targets)
		{
			TargetWrapper appropriateTarget = GetAppropriateTarget(ability.Data, item.Entity);
			if ((!flag || WarhammerGeometryUtils.DistanceToInCells(pathNode.node.Vector3Position, sizeRect, vector3Direction, appropriateTarget.Point, appropriateTarget.SizeRect, appropriateTarget.Forward) <= rangeCells) && ability.Data.CanTargetFromNode(pathNode.node, null, appropriateTarget, out var _, out var _, pathNode.direction) && ability.Data.IsTargetInsideRestrictedFiringArc(appropriateTarget, pathNode.node, pathNode.direction))
			{
				(Ability, MechanicEntity) key = (ability, item.Entity);
				if (!_abilityValueCache.TryGetValue(key, out var value))
				{
					value = brain.GetAbilityValue(ability.Data, item.Entity);
					_abilityValueCache[key] = value;
				}
				num = Math.Max(value, num);
			}
		}
		return num;
	}

	private TargetWrapper GetAppropriateTarget(AbilityData ability, MechanicEntity enemy)
	{
		if (ability.Blueprint.GetComponent<AbilityCustomStarshipNPCTorpedoLaunch>() != null)
		{
			return new TargetWrapper(enemy.Position + 5 * enemy.SizeRect.Width * enemy.Forward.ToCellSizedVector());
		}
		return new TargetWrapper(enemy);
	}
}
