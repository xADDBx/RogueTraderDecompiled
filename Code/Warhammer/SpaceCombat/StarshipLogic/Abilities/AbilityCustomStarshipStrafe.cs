using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Pathfinding;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[TypeId("c2bd270589e578143b8c700d785013eb")]
public class AbilityCustomStarshipStrafe : AbilityCustomLogic, ICustomShipPathProvider, IAbilityTargetRestriction
{
	[SerializeField]
	private GameObject NodeMarker;

	[SerializeField]
	private float strafeTime = 0.25f;

	[SerializeField]
	private bool AllowBulking;

	[SerializeField]
	[Range(0f, 1f)]
	[ShowIf("AllowBulking")]
	private float visualCellPenetration;

	[SerializeField]
	[ShowIf("AllowBulking")]
	private float fallBackTime;

	[SerializeField]
	[ShowIf("AllowBulking")]
	private float RamDamageMod;

	[SerializeField]
	private ActionList StarshipActionsOnFinish;

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		if (!(context.Caster is StarshipEntity starship))
		{
			yield break;
		}
		CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)AstarPath.active.GetNearest(target.Point).node;
		Vector3 startPosition = starship.Position;
		Vector3 endPosition = customGridNodeBase.Vector3Position;
		Vector3 returnPosition = (((endPosition - startPosition).magnitude > GraphParamsMechanicsCache.GridCellSize * 1.5f) ? Vector3.Lerp(startPosition, endPosition, 0.5f) : startPosition);
		StarshipEntity bulkingTarget = GetEnemyAtNode(starship, customGridNodeBase);
		double startTime = Game.Instance.TimeController.GameTime.TotalMilliseconds;
		while (true)
		{
			float num = (float)(Game.Instance.TimeController.GameTime.TotalMilliseconds - startTime) / 1000f;
			if (num >= strafeTime)
			{
				break;
			}
			starship.Position = Vector3.Lerp(startPosition, endPosition, num / strafeTime);
			if (bulkingTarget != null && (endPosition - starship.Position).magnitude <= visualCellPenetration * GraphParamsMechanicsCache.GridCellSize)
			{
				startPosition = starship.Position;
				break;
			}
			yield return null;
		}
		if (bulkingTarget != null)
		{
			yield return null;
			AbilityCustomStarshipRam.RamDamageBase(starship, bulkingTarget, 0, 0, RamDamageMod);
			endPosition = returnPosition;
			startTime = Game.Instance.TimeController.GameTime.TotalMilliseconds;
			while (true)
			{
				float num2 = (float)(Game.Instance.TimeController.GameTime.TotalMilliseconds - startTime) / 1000f;
				if (num2 >= fallBackTime)
				{
					break;
				}
				float num3 = num2 / fallBackTime;
				starship.Position = Vector3.Lerp(startPosition, endPosition, Mathf.Sin(MathF.PI / 180f * num3 * 90f));
				yield return null;
			}
		}
		starship.Position = endPosition;
		starship.View.AgentASP.Blocker.BlockAtCurrentPosition();
		yield return null;
		using (context.GetDataScope(starship.ToITargetWrapper()))
		{
			StarshipActionsOnFinish.Run();
		}
		yield return new AbilityDeliveryTarget(target);
	}

	public Dictionary<GraphNode, CustomPathNode> GetCustomPath(StarshipEntity starship, Vector3 pos, Vector3 dir)
	{
		Dictionary<GraphNode, CustomPathNode> customPathNodes = new Dictionary<GraphNode, CustomPathNode>();
		CustomGridNodeBase starshipNode = (CustomGridNodeBase)AstarPath.active.GetNearest(pos).node;
		int direction = CustomGraphHelper.GuessDirection(dir);
		AddLine(CustomGraphHelper.LeftNeighbourDirection[CustomGraphHelper.LeftNeighbourDirection[direction]], 2);
		AddLine(CustomGraphHelper.RightNeighbourDirection[CustomGraphHelper.RightNeighbourDirection[direction]], 2);
		return customPathNodes;
		void AddLine(int lineDir, int length)
		{
			CustomGridNodeBase customGridNodeBase = starshipNode;
			for (int i = 0; i < length; i++)
			{
				customGridNodeBase = customGridNodeBase.GetNeighbourAlongDirection(lineDir);
				CustomPathNode value = new CustomPathNode
				{
					Node = customGridNodeBase,
					Direction = direction,
					Parent = null,
					Marker = NodeMarker
				};
				bool flag = false;
				if (!starship.Navigation.CanStand(customGridNodeBase, direction))
				{
					if (!AllowBulking || GetEnemyAtNode(starship, customGridNodeBase) == null)
					{
						break;
					}
					flag = true;
				}
				customPathNodes.Add(customGridNodeBase, value);
				if (flag)
				{
					break;
				}
			}
		}
	}

	private StarshipEntity GetEnemyAtNode(StarshipEntity starship, CustomGridNodeBase node)
	{
		return Game.Instance.State.AllAwakeUnits.Where((AbstractUnitEntity p) => p is StarshipEntity && p.IsEnemy(starship)).Cast<StarshipEntity>().FirstOrDefault((StarshipEntity e) => !e.IsSoftUnit && e.IsUnitPositionContainsNode(e.Position, node));
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		if (!(ability.Caster is StarshipEntity starship))
		{
			return false;
		}
		GraphNode node = AstarPath.active.GetNearest(target.Point).node;
		Vector3 currentUnitDirection = UnitPredictionManager.Instance.CurrentUnitDirection;
		return GetCustomPath(starship, casterPosition, currentUnitDirection).ContainsKey(node);
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return LocalizedTexts.Instance.Reasons.CanOnlyTargetFinalNode;
	}
}
