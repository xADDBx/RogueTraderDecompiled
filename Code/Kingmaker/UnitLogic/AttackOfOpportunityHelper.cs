using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Covers;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public static class AttackOfOpportunityHelper
{
	private static class IsWaitingForIncomingAooChecker
	{
		public static MechanicEntity Unit;

		public static bool Check(UnitAttackOfOpportunity i)
		{
			if (!i.IsActed)
			{
				return i.TargetUnit == Unit;
			}
			return false;
		}
	}

	private static Func<UnitAttackOfOpportunity, bool> CheckIsWaitingForIncomingAoo = (UnitAttackOfOpportunity i) => IsWaitingForIncomingAooChecker.Check(i);

	public static IEnumerable<AttackOfOpportunityData> CalculateAttackOfOpportunity(this UnitMoveToProper move)
	{
		return move.Executor.CalculateAttackOfOpportunity(move.ForcedPath);
	}

	public static IEnumerable<AttackOfOpportunityData> CalculateAttackOfOpportunity(this UnitUseAbility useAbility)
	{
		return useAbility.Executor.CalculateAttackOfOpportunity(useAbility.Ability);
	}

	public static IEnumerable<AttackOfOpportunityData> CalculateAttackOfOpportunity(this BaseUnitEntity target, IEnumerable<Vector3> path)
	{
		return target.CalculateAttackOfOpportunity(path.Select((Vector3 i) => i.GetNearestNodeXZUnwalkable()).NotNull());
	}

	public static IEnumerable<AttackOfOpportunityData> CalculateAttackOfOpportunity(this BaseUnitEntity target, Path path)
	{
		List<GraphNode> path2 = path.path;
		if (path2 == null || path2.Count <= 0)
		{
			return target.CalculateAttackOfOpportunity(path.vectorPath);
		}
		return target.CalculateAttackOfOpportunity(path.path);
	}

	public static IEnumerable<AttackOfOpportunityData> CalculateAttackOfOpportunity(this BaseUnitEntity target, IEnumerable<GraphNode> path)
	{
		CustomGridNodeBase prevNode = target.CurrentUnwalkableNode;
		if (prevNode == null)
		{
			yield break;
		}
		foreach (GraphNode item in path)
		{
			CustomGridNodeBase node = (CustomGridNodeBase)item;
			if (node == prevNode)
			{
				continue;
			}
			foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
			{
				if (allBaseAwakeUnit is UnitEntity unitEntity && allBaseAwakeUnit.IsInCombat && unitEntity != target)
				{
					WeaponSlot threatHand = unitEntity.GetThreatHand();
					if ((threatHand == null || !threatHand.HasShield || threatHand.GetAttackOfOpportunityAbility(unitEntity) == null || !unitEntity.HasMechanicFeature(MechanicsFeatureType.DisableAttacksOfOpportunityForShield)) && unitEntity.CanMakeAttackOfOpportunity(target) && unitEntity.IsThreat(target, prevNode) && !unitEntity.IsThreat(target, node))
					{
						yield return new AttackOfOpportunityData(unitEntity, prevNode.Vector3Position);
					}
				}
			}
			prevNode = node;
		}
	}

	public static IEnumerable<AttackOfOpportunityData> CalculateAttackOfOpportunity(this BaseUnitEntity target, AbilityData ability)
	{
		if (ability.UsingInThreateningArea != 0 || (ability.UsingInThreateningArea == BlueprintAbility.UsingInThreateningAreaType.WillCauseAOO && target.HasMechanicFeature(MechanicsFeatureType.PsychicPowersDoNotProvokeAoO) && ability.Blueprint.AbilityParamsSource == WarhammerAbilityParamsSource.PsychicPower))
		{
			yield break;
		}
		foreach (BaseUnitEntity engagedByUnit in target.GetEngagedByUnits())
		{
			if (engagedByUnit.CanMakeAttackOfOpportunity(target))
			{
				yield return new AttackOfOpportunityData(engagedByUnit, target.Position, ability.Blueprint);
			}
		}
	}

	public static bool IsThreat(this BaseUnitEntity attacker, BaseUnitEntity target, CustomGridNodeBase targetNode = null)
	{
		if (targetNode == null)
		{
			targetNode = target.CurrentUnwalkableNode;
		}
		if (!attacker.State.CanAct || !attacker.IsEnemy(target))
		{
			return false;
		}
		WeaponSlot threatHand = attacker.GetThreatHand();
		if (threatHand?.GetAttackOfOpportunityAbility(attacker) == null)
		{
			return false;
		}
		return attacker.IsAttackOfOpportunityReach(target, targetNode, threatHand);
	}

	public static bool IsThreat(this BaseUnitEntity attacker, GraphNode targetNode, IntRect sizeRect = default(IntRect))
	{
		return attacker.IsThreat(targetNode, attacker.Position, sizeRect);
	}

	public static bool IsThreat(this BaseUnitEntity attacker, GraphNode targetNode, Vector3 attackerPosition, IntRect targetSize = default(IntRect))
	{
		if (!attacker.State.CanAct || (bool)attacker.GetMechanicFeature(MechanicsFeatureType.Hidden))
		{
			return false;
		}
		WeaponSlot threatHand = attacker.GetThreatHand();
		if (threatHand?.GetAttackOfOpportunityAbility(attacker) == null)
		{
			return false;
		}
		return attacker.IsAttackOfOpportunityReach(targetNode, attackerPosition, targetSize, threatHand);
	}

	public static bool IsAttackOfOpportunityReach(this BaseUnitEntity attacker, BaseUnitEntity target, CustomGridNodeBase targetNode, WeaponSlot hand)
	{
		return attacker.IsAttackOfOpportunityReach(attacker.CurrentUnwalkableNode, target, targetNode, hand);
	}

	public static bool IsAttackOfOpportunityReach(this BaseUnitEntity attacker, CustomGridNodeBase attackerNode, BaseUnitEntity target, CustomGridNodeBase targetNode, WeaponSlot hand)
	{
		using (ProfileScope.New("IsAttackOfOpportunityReach"))
		{
			int attackOfOpportunityThreatingRange = hand.GetAttackOfOpportunityThreatingRange(attacker);
			return attacker.InRangeInCells(targetNode.Vector3Position, target.SizeRect, attackOfOpportunityThreatingRange) && attacker.Vision.HasLOS(targetNode, attackerNode.Vector3Position + LosCalculations.EyeShift) && LosCalculations.HasMeleeLos(attacker.Position, attacker.SizeRect, targetNode.Vector3Position, target.SizeRect);
		}
	}

	public static bool CanMakeAttackOfOpportunity(this BaseUnitEntity attacker, BaseUnitEntity target)
	{
		if (Game.Instance.TurnController.CurrentUnit == attacker || (bool)attacker.Features.DisableAttacksOfOpportunity)
		{
			return false;
		}
		if (target.GetMechanicFeature(MechanicsFeatureType.DoNotProvokeAttacksOfOpportunity).Value)
		{
			return false;
		}
		if (!attacker.IsEnemy(target))
		{
			return false;
		}
		if (target.LifeState.IsDead)
		{
			return false;
		}
		if (!attacker.CombatState.CanActInCombat || !attacker.CombatState.CanAttackOfOpportunity || !attacker.State.CanAct || attacker.IsInvisible)
		{
			return false;
		}
		return !AbstractUnitCommand.CommandTargetUntargetable(attacker, target);
	}

	[CanBeNull]
	public static HashSet<GraphNode> GetThreateningArea(this BaseUnitEntity unit, WeaponSlot hand = null)
	{
		if (hand == null)
		{
			hand = unit.GetThreatHand();
		}
		if (hand?.GetAttackOfOpportunityAbility(unit) == null)
		{
			return null;
		}
		int attackOfOpportunityThreatingRange = hand.GetAttackOfOpportunityThreatingRange(unit);
		HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
		GridPatterns.AddCircleNodes(hashSet, attackOfOpportunityThreatingRange);
		HashSet<GraphNode> hashSet2 = TempHashSet.Get<GraphNode>();
		foreach (CustomGridNodeBase occupiedNode in unit.GetOccupiedNodes())
		{
			int xCoordinateInGrid = occupiedNode.XCoordinateInGrid;
			int zCoordinateInGrid = occupiedNode.ZCoordinateInGrid;
			CustomGridGraph customGridGraph = (CustomGridGraph)occupiedNode.Graph;
			foreach (Vector2Int item in hashSet)
			{
				CustomGridNodeBase node = customGridGraph.GetNode(xCoordinateInGrid + item.x, zCoordinateInGrid + item.y);
				if (node != null && !hashSet2.Contains(node) && (float)CustomGraphHelper.GetWarhammerCellDistance(occupiedNode, node) <= unit.Corpulence + (float)attackOfOpportunityThreatingRange && occupiedNode.HasMeleeLos(node))
				{
					hashSet2.Add(node);
				}
			}
		}
		return hashSet2;
	}

	public static bool IsWaitingForIncomingAttackOfOpportunity([NotNull] this MechanicEntity unit)
	{
		IsWaitingForIncomingAooChecker.Unit = unit;
		if (TurnController.IsInTurnBasedCombat())
		{
			return UnitAttackOfOpportunity.AllActive.HasItem(CheckIsWaitingForIncomingAoo);
		}
		return false;
	}

	public static BlueprintAbility GetAttackOfOpportunityAbility(this WeaponSlot slot, BaseUnitEntity unit)
	{
		BlueprintAbility blueprintAbility = slot.AttackOfOpportunityAbility;
		if (blueprintAbility == null)
		{
			UnitPartAttackOfOpportunityModifier optional = unit.Parts.GetOptional<UnitPartAttackOfOpportunityModifier>();
			if (optional != null && optional.EnableAndPrioritizeRangedAttack)
			{
				blueprintAbility = slot.MaybeWeapon?.Blueprint.WeaponAbilities.Ability1.Ability;
			}
		}
		return blueprintAbility;
	}

	public static int GetAttackOfOpportunityThreatingRange(this WeaponSlot slot, BaseUnitEntity unit)
	{
		int result = slot.AttackRange;
		if (slot.IsRanged)
		{
			UnitPartAttackOfOpportunityModifier optional = unit.Parts.GetOptional<UnitPartAttackOfOpportunityModifier>();
			if (optional != null && optional.EnableAndPrioritizeRangedAttack)
			{
				result = 1;
			}
		}
		return result;
	}
}
