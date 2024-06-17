using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("f059179ac7a24674186a7c001b912d2c")]
public class AbilityCustomStarshipNPCTorpedoLaunch : AbilityCustomLogic
{
	[SerializeField]
	private bool DoubleSpawnMode;

	[SerializeField]
	[HideIf("DoubleSpawnMode")]
	private int m_SpawnRotateLimit;

	[SerializeField]
	private BlueprintStarshipReference m_TorpedoBlueprint;

	[SerializeField]
	private ActionList m_ActionsOnTorpedo;

	[SerializeField]
	private ActionList m_ActionsOnSelf;

	public BlueprintStarship TorpedoBlueprint => m_TorpedoBlueprint?.Get();

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		if (!(context.MaybeCaster is StarshipEntity starship))
		{
			yield break;
		}
		if (DoubleSpawnMode)
		{
			var (customGridNodeBase, customGridNodeBase2) = GetTorpedoDoubleNode(starship);
			if (customGridNodeBase == null && customGridNodeBase2 == null)
			{
				yield break;
			}
			if (customGridNodeBase != null)
			{
				SpawnTorpedo(context, customGridNodeBase.Vector3Position, starship);
			}
			if (customGridNodeBase2 != null)
			{
				SpawnTorpedo(context, customGridNodeBase2.Vector3Position, starship);
			}
		}
		else
		{
			CustomGridNodeBase torpedoSpawnNode = GetTorpedoSpawnNode(starship, target, m_SpawnRotateLimit, 0);
			if (torpedoSpawnNode == null)
			{
				yield break;
			}
			SpawnTorpedo(context, torpedoSpawnNode.Vector3Position, starship);
		}
		yield return new AbilityDeliveryTarget(target);
	}

	private void SpawnTorpedo(AbilityExecutionContext context, Vector3 pos, StarshipEntity starship)
	{
		BaseUnitEntity torpedo = WarhammerContextActionSpawnChildStarship.SpawnStarship(TorpedoBlueprint, pos, null, starship);
		EventBus.RaiseEvent(delegate(ITorpedoSpawnHandler i)
		{
			i.HandleTorpedoSpawn(torpedo);
		});
		using (context.GetDataScope(torpedo.ToITargetWrapper()))
		{
			m_ActionsOnTorpedo.Run();
		}
		using (context.GetDataScope(starship.ToITargetWrapper()))
		{
			m_ActionsOnSelf.Run();
		}
	}

	public static CustomGridNodeBase GetTorpedoSpawnNode(StarshipEntity starship, TargetWrapper target, int rotateLimit, int extraDistanceLimit)
	{
		Vector3 vector3Direction = CustomGraphHelper.GetVector3Direction(starship.GetDirection());
		Vector3 target2 = target.Point - starship.Position;
		Vector3 spawnVector = Vector3.RotateTowards(vector3Direction, target2, MathF.PI / 180f * (float)rotateLimit, 0f);
		return GetSpawnNode(starship.Position, spawnVector, starship.SizeRect.Width, extraDistanceLimit);
	}

	public static (CustomGridNodeBase n1, CustomGridNodeBase n2) GetTorpedoDoubleNode(StarshipEntity starship)
	{
		Vector3 vector3Direction = CustomGraphHelper.GetVector3Direction(starship.GetDirection());
		Vector3 spawnVector = Quaternion.Euler(0f, 45f, 0f) * vector3Direction;
		Vector3 spawnVector2 = Quaternion.Euler(0f, -45f, 0f) * vector3Direction;
		CustomGridNodeBase spawnNode = GetSpawnNode(starship.Position, spawnVector, starship.SizeRect.Width, 0);
		CustomGridNodeBase spawnNode2 = GetSpawnNode(starship.Position, spawnVector2, starship.SizeRect.Width, 0);
		return (n1: spawnNode, n2: spawnNode2);
	}

	public static CustomGridNodeBase GetSpawnNode(Vector3 position, Vector3 spawnVector, int metaTileSize, int extraDistanceLimit, int boardOffset = 0, int shipDirection = 0)
	{
		int direction = CustomGraphHelper.GuessDirection(spawnVector);
		CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)AstarPath.active.GetNearest(position).node;
		for (int i = 0; i < boardOffset; i++)
		{
			customGridNodeBase = customGridNodeBase.GetNeighbourAlongDirection(shipDirection);
		}
		for (int num = 0; num > boardOffset; num--)
		{
			customGridNodeBase = customGridNodeBase.GetNeighbourAlongDirection(CustomGraphHelper.OppositeDirections[shipDirection]);
		}
		bool flag = true;
		int num2 = 0;
		while (flag && num2 < metaTileSize + extraDistanceLimit)
		{
			customGridNodeBase = customGridNodeBase.GetNeighbourAlongDirection(direction);
			flag = WarhammerBlockManager.Instance.NodeContainsAny(customGridNodeBase);
			num2++;
		}
		if (!flag)
		{
			return customGridNodeBase;
		}
		return null;
	}
}
