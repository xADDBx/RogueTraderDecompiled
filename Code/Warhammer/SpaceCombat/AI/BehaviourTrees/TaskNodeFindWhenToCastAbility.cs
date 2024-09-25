using System;
using System.Collections.Generic;
using Kingmaker.AI;
using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.BehaviourTrees.Nodes;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.EntitySystem;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.Random;

namespace Warhammer.SpaceCombat.AI.BehaviourTrees;

public class TaskNodeFindWhenToCastAbility : TaskNode
{
	private class AILogAbilityPlannedToCast : AILogObject
	{
		private readonly AbilityData ability;

		private readonly CustomGridNodeBase node;

		public AILogAbilityPlannedToCast(AbilityData ability, CustomGridNodeBase node)
		{
			this.ability = ability;
			this.node = node;
		}

		public override string GetLogString()
		{
			return $"Plan to cast {ability} on {node}";
		}
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		AILogger.Instance.Log(AILogNode.Start(this));
		SpaceCombatDecisionContext context = (SpaceCombatDecisionContext)blackboard.DecisionContext;
		Dictionary<Ability, AbilitySettings.AbilityCastSpotType> settings = new Dictionary<Ability, AbilitySettings.AbilityCastSpotType>();
		context.Unit.Abilities.RawFacts.ForEach(delegate(Ability f)
		{
			AbilitySettings starshipAbilitySettings = context.Unit.Brain.GetStarshipAbilitySettings(f.Blueprint);
			settings[f] = starshipAbilitySettings?.AbilityCastSpot ?? AbilitySettings.AbilityCastSpotType.Random;
		});
		context.PathNodesWithAbilities.Clear();
		Dictionary<Ability, ShipPath.DirectionalPathNode> dictionary = new Dictionary<Ability, ShipPath.DirectionalPathNode>();
		foreach (KeyValuePair<Ability, AbilitySettings.AbilityCastSpotType> item in settings)
		{
			SelectWhenToCast(context, item.Key, item.Value, dictionary);
		}
		foreach (KeyValuePair<Ability, ShipPath.DirectionalPathNode> item2 in dictionary)
		{
			if (!context.PathNodesWithAbilities.TryGetValue(item2.Value, out var value))
			{
				value = new List<Ability>();
				context.PathNodesWithAbilities[item2.Value] = value;
			}
			value.Add(item2.Key);
			AILogger.Instance.Log(new AILogAbilityPlannedToCast(item2.Key.Data, item2.Value.node));
		}
		return Status.Success;
	}

	private void SelectWhenToCast(SpaceCombatDecisionContext context, Ability ability, AbilitySettings.AbilityCastSpotType castSpotType, Dictionary<Ability, ShipPath.DirectionalPathNode> whenToCastAbilityDict)
	{
		int num = 0;
		foreach (ShipPath.DirectionalPathNode item in context.BestPath)
		{
			num = Math.Max(num, context.AbilityValueCache.GetValue(item, ability));
		}
		if (num == 0)
		{
			return;
		}
		int num2 = int.MaxValue;
		int num3 = 0;
		int num4 = context.Unit.Brain.GetStarshipAbilitySettings(ability.Blueprint)?.OptimumDistance ?? 0;
		foreach (ShipPath.DirectionalPathNode item2 in context.BestPath)
		{
			if (context.AbilityValueCache.GetValue(item2, ability) < num)
			{
				continue;
			}
			switch (castSpotType)
			{
			case AbilitySettings.AbilityCastSpotType.Random:
				num3++;
				if (PFStatefulRandom.SpaceCombat.Range(0, num3) == 0)
				{
					whenToCastAbilityDict[ability] = item2;
				}
				break;
			case AbilitySettings.AbilityCastSpotType.Earlier:
				whenToCastAbilityDict[ability] = item2;
				return;
			case AbilitySettings.AbilityCastSpotType.Later:
				whenToCastAbilityDict[ability] = item2;
				break;
			case AbilitySettings.AbilityCastSpotType.CloserToTarget:
				foreach (TargetInfo enemy in context.Enemies)
				{
					int num6 = enemy.Entity.DistanceToInCells(item2.node.Vector3Position);
					if (num6 < num2)
					{
						num2 = num6;
						whenToCastAbilityDict[ability] = item2;
					}
				}
				break;
			case AbilitySettings.AbilityCastSpotType.CloserToOptimum:
				foreach (TargetInfo enemy2 in context.Enemies)
				{
					int num5 = enemy2.Entity.DistanceToInCells(item2.node.Vector3Position);
					if (Math.Abs(num5 - num4) < Math.Abs(num2 - num4))
					{
						num2 = num5;
						whenToCastAbilityDict[ability] = item2;
					}
				}
				break;
			}
		}
	}
}
