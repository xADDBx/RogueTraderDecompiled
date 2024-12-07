using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Formations;

public static class PartyFormationHelper
{
	public const int UnitsInFormationCapacity = 20;

	public static readonly NNConstraint Constraint = NNConstraint.Default;

	public static Vector2 GetOffset(Vector2[] positions, int index)
	{
		return ((index >= 0 && index < positions.Length) ? positions[index] : Vector2.zero) * BlueprintRoot.Instance.Formations.FormationsScale;
	}

	public static void SetOffset(Vector2[] positions, int index, Vector2 pos)
	{
		if (index < 0 || index >= positions.Length)
		{
			PFLog.Default.Warning("Cannot find index in formation's position arroy");
		}
		else
		{
			positions[index] = pos / BlueprintRoot.Instance.Formations.FormationsScale;
		}
	}

	public static void FillFormationPositions(Vector3 pos, FormationAnchor anchor, Vector3 direction, IList<BaseUnitEntity> units, IList<BaseUnitEntity> selectedUnits, IImmutablePartyFormation formation, Span<Vector3> resultPositions, float spaceFactor = 1f, bool forceRelax = false, int anchorUnitIndex = -1)
	{
		FillFormationPositions(pos, anchor, direction, ((IEnumerable<BaseUnitEntity>)units).Select((Func<BaseUnitEntity, AbstractUnitEntity>)((BaseUnitEntity x) => x)).ToList(), ((IEnumerable<BaseUnitEntity>)selectedUnits).Select((Func<BaseUnitEntity, AbstractUnitEntity>)((BaseUnitEntity x) => x)).ToList(), formation, resultPositions, spaceFactor, forceRelax, anchorUnitIndex);
	}

	public static void FillFormationPositions(Vector3 pos, FormationAnchor anchor, Vector3 direction, IList<AbstractUnitEntity> units, IList<AbstractUnitEntity> selectedUnits, IImmutablePartyFormation formation, Span<Vector3> resultPositions, float spaceFactor = 1f, bool forceRelax = false, int anchorUnitIndex = -1)
	{
		int index = ((anchor == FormationAnchor.SelectedUnit && anchorUnitIndex >= 0 && anchorUnitIndex < units.Count) ? anchorUnitIndex : 0);
		CustomGridNodeBase nearestNodeXZ = units[index].GetNearestNodeXZ();
		if (nearestNodeXZ != null)
		{
			Constraint.constrainArea = true;
			Constraint.area = (int)nearestNodeXZ.Area;
		}
		else
		{
			Constraint.constrainArea = false;
		}
		CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(pos, null, Constraint).node;
		if (customGridNodeBase != null && !customGridNodeBase.ContainsPoint(pos))
		{
			pos = customGridNodeBase.Vector3Position;
		}
		direction.y = 0f;
		Quaternion quaternion = Quaternion.LookRotation(direction);
		if (units.Count > 0)
		{
			if (units.Count == 1)
			{
				anchor = FormationAnchor.Center;
			}
			Vector3 vector = quaternion * GetFormationAnchorPoint().To3D();
			NNInfo nearestNode = ObstacleAnalyzer.GetNearestNode(pos);
			if (nearestNode.node == null)
			{
				nearestNode = ObstacleAnalyzer.GetNearestNode(pos, null, ObstacleAnalyzer.UnwalkableXZConstraint);
			}
			Vector3 position = nearestNode.position;
			for (int i = 0; i < units.Count; i++)
			{
				Vector3 end = position - vector + quaternion * formation.GetOffset(i, units[i]).To3D();
				Linecast.LinecastGrid(nearestNode.node.Graph, position, end, nearestNode.node, out var hit, ObstacleAnalyzer.DefaultXZConstraint, ref Linecast.HasConnectionTransition.Instance);
				Vector3 point = hit.point;
				GraphNode node = hit.node;
				resultPositions[i] = ObstacleAnalyzer.FindClosestPointToStandOn(point, units[i].MovementAgent.Corpulence, (CustomGridNodeBase)node);
			}
		}
		Vector2 GetFormationAnchorPoint()
		{
			switch (anchor)
			{
			case FormationAnchor.Center:
				return GetFormationCenter();
			case FormationAnchor.Front:
				return GetFormationFront();
			case FormationAnchor.SelectedUnit:
				if (anchorUnitIndex >= 0 && anchorUnitIndex < units.Count)
				{
					return formation.GetOffset(anchorUnitIndex, units[anchorUnitIndex]);
				}
				break;
			}
			return Vector2.zero;
		}
		Vector2 GetFormationCenter()
		{
			Vector2 zero = Vector2.zero;
			for (int j = 0; j < units.Count; j++)
			{
				if (selectedUnits.Contains(units[j]))
				{
					Vector2 offset = formation.GetOffset(j, units[j]);
					zero += offset;
				}
			}
			if (selectedUnits.Count > 0)
			{
				zero /= (float)selectedUnits.Count;
			}
			return zero;
		}
		Vector2 GetFormationFront()
		{
			float num = float.MinValue;
			for (int k = 0; k < units.Count; k++)
			{
				if (selectedUnits.Contains(units[k]))
				{
					Vector2 offset2 = formation.GetOffset(k, units[k]);
					if (offset2.y > num)
					{
						num = offset2.y;
					}
				}
			}
			return new Vector2(0f, num);
		}
	}
}
