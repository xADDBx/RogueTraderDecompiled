using System;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Pathfinding;

[Serializable]
[TypeId("db6cc3d2ef294450a473fefd33f1c8bc")]
public class NavMeshFollower : EntityFactComponentDelegate, IAreaLoadingStagesHandler, ISubscriber, IRoundStartHandler, IHashable
{
	public void OnAreaScenesLoaded()
	{
	}

	public void OnAreaLoadingComplete()
	{
		MoveNavMeshToPlayer();
		Game.Instance.Player.PlayerShip.Navigation.UpdateReachableTiles(delegate
		{
		});
	}

	public void HandleRoundStart(bool isTurnBased)
	{
		MoveNavMeshToPlayer();
	}

	public void MoveNavMeshToPlayer()
	{
		CustomGridNodeBase customGridNodeBase = Game.Instance.Player.PlayerShip.GetOccupiedNodes().FirstOrDefault();
		CustomGridGraph customGridGraph = (CustomGridGraph)AstarPath.active.graphs.FirstOrDefault();
		if (customGridNodeBase != null && customGridGraph != null)
		{
			int num = (int)((customGridNodeBase.Vector3Position.x - customGridGraph.center.x) / customGridGraph.nodeSize);
			int num2 = (int)((customGridNodeBase.Vector3Position.z - customGridGraph.center.z) / customGridGraph.nodeSize);
			Vector3 vector = new Vector3((float)num * customGridGraph.nodeSize, 0f, (float)num2 * customGridGraph.nodeSize);
			customGridGraph?.RelocateNodes(customGridGraph.center + vector, Quaternion.identity, customGridGraph.nodeSize, customGridGraph.aspectRatio, customGridGraph.isometricAngle);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
