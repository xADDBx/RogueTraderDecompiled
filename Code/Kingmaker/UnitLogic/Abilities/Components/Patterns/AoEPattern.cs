using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Enums;
using Kingmaker.Pathfinding;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.UnitLogic.Abilities.Components.Patterns;

[Serializable]
public class AoEPattern
{
	public const float SameLevelDiff = 1.6f;

	public const float RayConeThickness = 0.3f;

	[SerializeField]
	private PatternType m_Type;

	[SerializeField]
	[ShowIf("IsCustom")]
	private BlueprintAttackPatternReference m_Blueprint;

	[HideIf("IsCustom")]
	[SerializeField]
	private int m_Radius;

	[ShowIf("IsCone")]
	[SerializeField]
	[RangeWithStep(30, 180, 30)]
	private int m_Angle;

	public PatternType Type
	{
		get
		{
			if (!IsCustom || Blueprint == null)
			{
				return m_Type;
			}
			return Blueprint.Type;
		}
	}

	public bool CanBeDirectional
	{
		get
		{
			PatternType type = m_Type;
			return type == PatternType.Ray || type == PatternType.Cone || type == PatternType.Sector;
		}
	}

	private bool IsCustom => m_Type == PatternType.Custom;

	private bool IsCone
	{
		get
		{
			if (m_Type != PatternType.Cone)
			{
				return m_Type == PatternType.Sector;
			}
			return true;
		}
	}

	private BlueprintAttackPattern Blueprint => m_Blueprint?.Get();

	public int Angle => m_Type switch
	{
		PatternType.Cone => m_Angle, 
		PatternType.Sector => m_Angle, 
		PatternType.Ray => 0, 
		PatternType.Custom => Blueprint.Angle, 
		PatternType.Circle => 360, 
		_ => throw new ArgumentOutOfRangeException(), 
	};

	public int Radius
	{
		get
		{
			if (m_Type != PatternType.Custom)
			{
				return m_Radius;
			}
			if (Blueprint.Type != PatternType.Custom)
			{
				return Blueprint.Radius;
			}
			IntRect bounds = Bounds;
			return Math.Max(Math.Max(bounds.xmax, bounds.ymax), Math.Max(-bounds.xmin, -bounds.ymin));
		}
	}

	public IntRect Bounds
	{
		get
		{
			using PatternGridData patternGridData = GetGridData(Vector2.up);
			using PatternGridData patternGridData2 = GetGridData((Vector2.up + Vector2.right).normalized);
			return IntRect.Union(patternGridData.Bounds, patternGridData2.Bounds);
		}
	}

	public PatternGridData GetGridData(Vector2 direction, Size entitySizeRect = Size.Medium)
	{
		float sqrMagnitude = direction.sqrMagnitude;
		if (sqrMagnitude > 1.1f || sqrMagnitude < 0.9f)
		{
			throw new ArgumentException("Need nonzero vector", "direction");
		}
		if (m_Type != PatternType.Custom)
		{
			return GridPatterns.ConstructPattern(m_Type, m_Radius, m_Angle, direction, entitySizeRect);
		}
		return Blueprint.GetOrientedNodes(direction);
	}

	public OrientedPatternData GetOriented(CustomGridNodeBase applicationNode, Vector3 direction)
	{
		return GetOriented(applicationNode, applicationNode, direction, isIgnoreLos: true, isIgnoreLevelDifference: true);
	}

	public PatternGridData GetOrientedGridData(CustomGridNodeBase checkLosFromNode, CustomGridNodeBase applicationNode, Vector3 direction, bool isIgnoreLos = false, bool isIgnoreLevelDifference = false, bool isDirectional = false, bool coveredTargetsOnly = false, bool useMeleeLos = false, Size entitySizeRect = Size.Medium)
	{
		Vector2 direction2 = (((double)direction.sqrMagnitude < 0.0001) ? Vector2.down : direction.To2D().normalized);
		float num = 0f;
		bool flag = Math.Abs(direction2.x) > Math.Abs(direction2.y);
		CustomGridGraph customGridGraph = (CustomGridGraph)applicationNode.Graph;
		if (isDirectional)
		{
			Vector3 normalized = direction.normalized;
			float nodeSize = customGridGraph.nodeSize;
			float num2 = (flag ? (nodeSize / Math.Abs(normalized.x)) : (nodeSize / Math.Abs(normalized.z)));
			num = (normalized * num2).y;
		}
		float num3 = (isIgnoreLevelDifference ? float.MaxValue : (isDirectional ? 0.3f : 1.6f));
		HashSet<Vector2Int> value;
		using (CollectionPool<HashSet<Vector2Int>, Vector2Int>.Get(out value))
		{
			PatternGridData pattern = GetGridData(direction2, entitySizeRect).Move(applicationNode.CoordinatesInGrid);
			NodeList nodeList = new NodeList(customGridGraph, in pattern);
			float num4 = checkLosFromNode.Vector3Position.y - applicationNode.Vector3Position.y;
			foreach (CustomGridNodeBase item in nodeList)
			{
				if (!coveredTargetsOnly || item.ContainsUnit())
				{
					Vector3 vector = item.Vector3Position - applicationNode.Vector3Position;
					float num7;
					if (isDirectional)
					{
						int num5 = (flag ? Mathf.Abs(applicationNode.XCoordinateInGrid - item.XCoordinateInGrid) : Mathf.Abs(applicationNode.ZCoordinateInGrid - item.ZCoordinateInGrid));
						float num6 = applicationNode.Vector3Position.y + (float)num5 * num + num4;
						num7 = Mathf.Abs(item.Vector3Position.y - num6);
					}
					else
					{
						num7 = Mathf.Abs(vector.y);
					}
					if (!(num7 > num3) && (isIgnoreLos || (useMeleeLos ? checkLosFromNode.HasMeleeLos(item) : LosCalculations.HasLos(checkLosFromNode, default(IntRect), item, default(IntRect)))))
					{
						value.Add(item.CoordinatesInGrid);
					}
				}
			}
			return PatternGridData.Create(value, disposable: true);
		}
	}

	public OrientedPatternData GetOriented(CustomGridNodeBase checkLosFromNode, [NotNull] CustomGridNodeBase applicationNode, Vector3 direction, bool isIgnoreLos = false, bool isIgnoreLevelDifference = false, bool isDirectional = false, bool coveredTargetsOnly = false, bool useMeleeLos = false)
	{
		PatternGridData orientedGridData = GetOrientedGridData(checkLosFromNode, applicationNode, direction, isIgnoreLos, isIgnoreLevelDifference, isDirectional, coveredTargetsOnly, useMeleeLos);
		CustomGridGraph graph = (CustomGridGraph)applicationNode.Graph;
		return new OrientedPatternData(orientedGridData.Select((Vector2Int i) => graph.GetNode(i.x, i.y)).ToTempList(), applicationNode);
	}

	public static AoEPattern Ray(int length)
	{
		return new AoEPattern
		{
			m_Type = PatternType.Ray,
			m_Radius = length
		};
	}

	public static AoEPattern Cone(int angle, int radius)
	{
		return new AoEPattern
		{
			m_Type = PatternType.Cone,
			m_Radius = radius,
			m_Angle = angle
		};
	}

	public static AoEPattern Sector(int angle, int radius)
	{
		return new AoEPattern
		{
			m_Type = PatternType.Sector,
			m_Radius = radius,
			m_Angle = angle
		};
	}

	public static AoEPattern Circle(int radius)
	{
		return new AoEPattern
		{
			m_Type = PatternType.Circle,
			m_Radius = radius
		};
	}

	public static AoEPattern CopyAndOverrideRadius(AoEPattern pattern, int radius)
	{
		return new AoEPattern
		{
			m_Blueprint = pattern.m_Blueprint,
			m_Type = pattern.m_Type,
			m_Radius = radius,
			m_Angle = pattern.m_Angle
		};
	}

	public static AoEPattern FromBlueprint(BlueprintAttackPattern blueprint)
	{
		return new AoEPattern
		{
			m_Type = PatternType.Custom,
			m_Blueprint = blueprint.ToReference<BlueprintAttackPatternReference>()
		};
	}

	public static Vector3 GetCastDirection(PatternType patternType, CustomGridNodeBase casterNode, CustomGridNodeBase castNode, CustomGridNodeBase targetNode)
	{
		Vector3 result = targetNode.Vector3Position - casterNode.Vector3Position;
		if (patternType == PatternType.Ray || patternType == PatternType.Cone || patternType == PatternType.Sector)
		{
			Vector3 vector = (((targetNode.Vector3Position - castNode.Vector3Position).sqrMagnitude < 1f) ? (targetNode.Vector3Position - casterNode.Vector3Position) : (targetNode.Vector3Position - castNode.Vector3Position));
			result = new Vector3(vector.x, result.y, vector.z);
		}
		return result;
	}
}
