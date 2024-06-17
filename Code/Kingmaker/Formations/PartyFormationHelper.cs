using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Formations;

public static class PartyFormationHelper
{
	public const int UnitsInFormationCapacity = 20;

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

	public static void FillFormationPositions(Vector3 pos, FormationAnchor anchor, Vector3 direction, IList<BaseUnitEntity> units, IList<BaseUnitEntity> selectedUnits, IImmutablePartyFormation formation, Span<Vector3> resultPositions, float spaceFactor = 1f, bool forceRelax = false)
	{
		FillFormationPositions(pos, anchor, direction, ((IEnumerable<BaseUnitEntity>)units).Select((Func<BaseUnitEntity, AbstractUnitEntity>)((BaseUnitEntity x) => x)).ToList(), ((IEnumerable<BaseUnitEntity>)selectedUnits).Select((Func<BaseUnitEntity, AbstractUnitEntity>)((BaseUnitEntity x) => x)).ToList(), formation, resultPositions, spaceFactor, forceRelax);
	}

	public static void FillFormationPositions(Vector3 pos, FormationAnchor anchor, Vector3 direction, IList<AbstractUnitEntity> units, IList<AbstractUnitEntity> selectedUnits, IImmutablePartyFormation formation, Span<Vector3> resultPositions, float spaceFactor = 1f, bool forceRelax = false)
	{
		direction.y = 0f;
		Quaternion quaternion = Quaternion.LookRotation(direction);
		Vector2 zero = Vector2.zero;
		float num = float.MinValue;
		for (int i = 0; i < units.Count; i++)
		{
			if (selectedUnits.Contains(units[i]))
			{
				Vector2 offset = formation.GetOffset(i, units[i]);
				if (offset.y > num)
				{
					num = offset.y;
				}
				zero += offset;
			}
		}
		if (selectedUnits.Count > 0)
		{
			zero /= (float)selectedUnits.Count;
		}
		if (units.Count > 0)
		{
			Vector3 vector = new Vector3(0f, 0f, 0f);
			if (units.Count == 1)
			{
				anchor = FormationAnchor.Center;
			}
			vector = anchor switch
			{
				FormationAnchor.Center => quaternion * zero.To3D(), 
				FormationAnchor.Front => quaternion * new Vector2(0f, num).To3D(), 
				_ => vector, 
			};
			NNInfo nearestNode = ObstacleAnalyzer.GetNearestNode(pos);
			Vector3 position = nearestNode.position;
			for (int j = 0; j < units.Count; j++)
			{
				Vector3 end = position - vector + quaternion * formation.GetOffset(j, units[j]).To3D();
				Linecast.LinecastGrid(nearestNode.node.Graph, position, end, nearestNode.node, out var hit, ObstacleAnalyzer.DefaultXZConstraint, ref Linecast.HasConnectionTransition.Instance);
				Vector3 point = hit.point;
				GraphNode node = hit.node;
				resultPositions[j] = ObstacleAnalyzer.FindClosestPointToStandOn(point, units[j].MovementAgent.Corpulence, (CustomGridNodeBase)node);
			}
		}
	}

	public static Vector3 FindFormationCenterFromOneUnit(FormationAnchor anchor, Vector3 direction, int unitIndex, Vector3 unitPosition, List<BaseUnitEntity> units, UnitReference[] selectedUnits)
	{
		if (units.Count <= 0)
		{
			return Vector3.zero;
		}
		if (selectedUnits.Length == 1)
		{
			return unitPosition;
		}
		direction.y = 0f;
		Quaternion quaternion = Quaternion.LookRotation(direction);
		IPartyFormation currentFormation = Game.Instance.Player.FormationManager.CurrentFormation;
		Vector2 zero = Vector2.zero;
		float num = float.MinValue;
		for (int i = 0; i < units.Count; i++)
		{
			if (selectedUnits.IndexOf(units[i].FromBaseUnitEntity()) != -1)
			{
				Vector2 offset = currentFormation.GetOffset(i, units[i]);
				if (offset.y > num)
				{
					num = offset.y;
				}
				zero += offset;
			}
		}
		if (selectedUnits.Length != 0)
		{
			zero /= (float)selectedUnits.Length;
		}
		if (!units.IsValidIndex(unitIndex))
		{
			return Vector3.zero;
		}
		Vector3 result = unitPosition - quaternion * currentFormation.GetOffset(unitIndex, units[unitIndex]).To3D();
		if (units.Count > 1)
		{
			switch (anchor)
			{
			case FormationAnchor.Center:
				result += quaternion * zero.To3D();
				break;
			case FormationAnchor.Front:
				result += quaternion * new Vector2(0f, num).To3D();
				break;
			}
		}
		return result;
	}
}
