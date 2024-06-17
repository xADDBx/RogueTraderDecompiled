using System.Collections.Generic;
using Kingmaker.AI.AreaScanning.Scoring;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Pathfinding;

namespace Kingmaker.AI.AreaScanning.TileScorers;

public class MinDistanceToEnemyTileScorer : TileScorer
{
	protected override Score CalculateClosinessScore(DecisionContext context, CustomGridNodeBase node)
	{
		IntRect sizeRect = context.Unit.SizeRect;
		AbilityData refAbility = context.ConsideringAbility ?? context.Ability;
		return new Score(1f / (float)DistanceToClosestEnemyInCells(node, sizeRect, context.HatedTargets, refAbility));
	}

	private int DistanceToClosestEnemyInCells(CustomGridNodeBase checkNode, IntRect rect, List<TargetInfo> enemyInfos, AbilityData refAbility)
	{
		int num = int.MaxValue;
		foreach (TargetInfo enemyInfo in enemyInfos)
		{
			int num2 = enemyInfo.Entity.DistanceToInCells(checkNode.Vector3Position, rect);
			if (num2 >= refAbility.MinRangeCells && num2 <= refAbility.RangeCells && num2 < num && refAbility.CanTargetFromNode(checkNode, enemyInfo.Node, CreateTargetWrapper(refAbility, enemyInfo.Entity), out var _, out var _))
			{
				num = num2;
			}
		}
		return num;
	}

	private TargetWrapper CreateTargetWrapper(AbilityData ability, MechanicEntity entity)
	{
		if (ability.TargetAnchor == AbilityTargetAnchor.Point)
		{
			return new TargetWrapper(entity.Position);
		}
		return new TargetWrapper(entity);
	}
}
