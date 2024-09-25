using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("da006e6d97ab3d14c8f0fdf4616fc61d")]
public class WarhammerContextActionRunToTarget : ContextAction
{
	public int maxRunDistance;

	public bool storeStartingPosition;

	public bool runToStoredPosition;

	public ActionList ActionsOnSuccess;

	public override string GetCaption()
	{
		return ((!runToStoredPosition) ? "Caster run to position close to target" : "Caster run to saved position") + " by straight line" + (storeStartingPosition ? " and starting position is stored" : "");
	}

	protected override void RunAction()
	{
		MechanicEntity caster = base.Context.MaybeOwner;
		MechanicEntity entity = base.Target.Entity;
		if (caster == null || entity == null)
		{
			return;
		}
		List<Vector3> list = new List<Vector3>();
		if (runToStoredPosition)
		{
			WarhammerUnitPartStorePosition warhammerUnitPartStorePosition = caster?.GetOptional<WarhammerUnitPartStorePosition>();
			if (warhammerUnitPartStorePosition == null)
			{
				Element.LogError(this, "WarhammerContextActionRunToTarget: no stored position");
				return;
			}
			list.Add(warhammerUnitPartStorePosition.storedPosition);
		}
		else
		{
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					Vector3 vector = entity.Position + new Vector3(GraphParamsMechanicsCache.GridCellSize * (float)i, 0f, GraphParamsMechanicsCache.GridCellSize * (float)j);
					if (CheckRunPath(caster, vector))
					{
						list.Add(vector);
					}
				}
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		if (storeStartingPosition)
		{
			caster.GetOrCreate<WarhammerUnitPartStorePosition>().storedPosition = caster.Position;
		}
		list.Sort((Vector3 a, Vector3 b) => (int)Mathf.Sign((a - caster.Position).sqrMagnitude - (b - caster.Position).sqrMagnitude));
		caster.Position = list.FirstItem();
		using (base.Context.GetDataScope((TargetWrapper)entity))
		{
			ActionsOnSuccess.Run();
		}
	}

	private bool CheckRunPath(MechanicEntity caster, Vector3 point)
	{
		NodeList nodes = GetOrientedPattern(caster.Position, point).Nodes;
		PriorityQueue<CustomGridNodeBase> priorityQueue = new PriorityQueue<CustomGridNodeBase>(DistanceComparer(caster), EqualityComparer<CustomGridNodeBase>.Default);
		foreach (CustomGridNodeBase item in nodes)
		{
			priorityQueue.Enqueue(item);
		}
		Dictionary<CustomGridNodeBase, BaseUnitEntity> dictionary = new Dictionary<CustomGridNodeBase, BaseUnitEntity>();
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.State.AllBaseUnits)
		{
			dictionary[(CustomGridNodeBase)(GraphNode)allBaseUnit.CurrentNode] = allBaseUnit;
		}
		CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)(GraphNode)ObstacleAnalyzer.GetNearestNode(point);
		if (dictionary.ContainsKey(customGridNodeBase))
		{
			return false;
		}
		CustomGridNodeBase customGridNodeBase2 = (CustomGridNodeBase)(GraphNode)caster.CurrentNode;
		while (priorityQueue.Count > 0)
		{
			if (dictionary.TryGetValue(customGridNodeBase2, out var value) && value.CombatGroup.IsEnemy(caster))
			{
				return false;
			}
			if (customGridNodeBase2 == customGridNodeBase)
			{
				return true;
			}
			CustomGridNodeBase customGridNodeBase3 = priorityQueue.Dequeue();
			if (!customGridNodeBase2.ContainsConnection(customGridNodeBase3))
			{
				return false;
			}
			customGridNodeBase2 = customGridNodeBase3;
		}
		return customGridNodeBase2 == customGridNodeBase;
	}

	public IComparer<CustomGridNodeBase> DistanceComparer(MechanicEntity caster)
	{
		return Comparer<CustomGridNodeBase>.Create((CustomGridNodeBase a, CustomGridNodeBase b) => Comparer<float>.Default.Compare(caster.DistanceToInCells(a.Vector3Position), caster.DistanceToInCells(b.Vector3Position)));
	}

	public OrientedPatternData GetOrientedPattern(Vector3 casterPos, Vector3 targetPos)
	{
		return GetOrientedRayPattern(casterPos, targetPos, maxRunDistance);
	}

	private OrientedPatternData GetOrientedRayPattern(Vector3 casterPos, Vector3 targetPos, int length)
	{
		CustomGridNodeBase actualCastNode = AoEPatternHelper.GetActualCastNode(base.AbilityContext.Caster, casterPos, targetPos, 1, 1);
		return AoEPattern.Ray(length).GetOriented(actualCastNode, targetPos - casterPos);
	}
}
