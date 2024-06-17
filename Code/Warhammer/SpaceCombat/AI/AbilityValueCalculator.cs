using System;
using System.Collections.Generic;
using Kingmaker.AI;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;

namespace Warhammer.SpaceCombat.AI;

public class AbilityValueCalculator
{
	private readonly SpaceCombatDecisionContext context;

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
		PartUnitBrain brain = context.Unit.Brain;
		if (!ability.Data.IsAvailable)
		{
			return num;
		}
		List<TargetInfo> targets = context.Enemies;
		(brain?.Blueprint as BlueprintStarshipBrain)?.TryOverrideTargets(context, ref targets);
		foreach (TargetInfo item in targets)
		{
			TargetWrapper appropriateTarget = GetAppropriateTarget(ability.Data, item.Entity);
			if (ability.Data.CanTargetFromNode(pathNode.node, null, appropriateTarget, out var _, out var _, pathNode.direction) && ability.Data.IsTargetInsideRestrictedFiringArc(appropriateTarget, pathNode.node, pathNode.direction))
			{
				num = Math.Max(brain.GetAbilityValue(ability.Data, item.Entity), num);
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
