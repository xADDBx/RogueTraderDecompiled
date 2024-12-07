using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.Pathfinding;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints;

[TypeId("9a73e1656efa4af4595cc21749b77820")]
public class BlueprintAttackPattern : BlueprintScriptableObject
{
	private delegate Vector2Int RotateDelegate(Vector2Int v);

	private const float k_DiagonalAngleInDegrees = 30f;

	private static readonly float s_AxisAngCos;

	private static readonly float s_DiagAngCos;

	public PatternType Type;

	[HideIf("IsCustom")]
	[SerializeField]
	[FormerlySerializedAs("Radius")]
	private int m_Radius;

	[ShowIf("HasAngle")]
	[SerializeField]
	[FormerlySerializedAs("Angle")]
	[RangeWithStep(30, 180, 30)]
	private int m_Angle;

	[ShowIf("CanBeDirectional")]
	[InfoBox("'Directional' spreading works only with Sector, Cone and Ray patterns")]
	[SerializeField]
	private bool m_Directional;

	[SerializeField]
	[ShowIf("IsCustom")]
	private CustomAttackPattern AxisAlignedPattern;

	[SerializeField]
	[ShowIf("IsCustom")]
	private CustomAttackPattern DiagonalAlignedPattern;

	[SerializeField]
	[ShowIf("IsCustom")]
	public bool ExcludeFirstNode;

	private readonly StaticCache<int, PatternGridData> m_Cache;

	private bool IsCustom => Type == PatternType.Custom;

	private bool HasAngle
	{
		get
		{
			if (Type != PatternType.Cone)
			{
				return Type == PatternType.Sector;
			}
			return true;
		}
	}

	public bool CanBeDirectional
	{
		get
		{
			PatternType type = Type;
			return type == PatternType.Ray || type == PatternType.Cone || type == PatternType.Sector;
		}
	}

	public int Radius
	{
		get
		{
			if (!IsCustom)
			{
				return m_Radius;
			}
			return 1;
		}
	}

	public int Angle => Type switch
	{
		PatternType.Cone => m_Angle, 
		PatternType.Sector => m_Angle, 
		PatternType.Ray => 0, 
		PatternType.Circle => 360, 
		PatternType.Custom => 360, 
		_ => 360, 
	};

	public bool IsDirectional
	{
		get
		{
			if (CanBeDirectional)
			{
				return m_Directional;
			}
			return false;
		}
	}

	static BlueprintAttackPattern()
	{
		float num = MathF.PI / 6f;
		s_AxisAngCos = Mathf.Cos(0.5f * (MathF.PI / 2f - num));
		s_DiagAngCos = Mathf.Cos(0.5f * num);
	}

	public BlueprintAttackPattern()
	{
		m_Cache = new StaticCache<int, PatternGridData>(ConstructPattern);
	}

	public PatternGridData GetOrientedNodes(Vector2 direction, Size entitySizeRect = Size.Medium)
	{
		float sqrMagnitude = direction.sqrMagnitude;
		if (sqrMagnitude > 1.1f || sqrMagnitude < 0.9f)
		{
			throw new ArgumentException("Need nonzero vector", "direction");
		}
		if (Type != PatternType.Custom)
		{
			return GridPatterns.ConstructPattern(Type, m_Radius, m_Angle, direction, entitySizeRect);
		}
		return GetCustomData(direction);
	}

	private PatternGridData GetCustomData(Vector2 direction)
	{
		int key = DetermineOrientation(direction);
		return m_Cache.Get(key);
	}

	private static int DetermineOrientation(Vector2 dir)
	{
		if (Vector2.Dot(dir, Vector2.up) > s_AxisAngCos)
		{
			return 2;
		}
		if (Vector2.Dot(dir, Vector2.left) > s_AxisAngCos)
		{
			return 3;
		}
		if (Vector2.Dot(dir, Vector2.down) > s_AxisAngCos)
		{
			return 0;
		}
		if (Vector2.Dot(dir, Vector2.right) > s_AxisAngCos)
		{
			return 1;
		}
		if (Vector2.Dot(dir, (Vector2.up + Vector2.right).normalized) > s_DiagAngCos)
		{
			return 5;
		}
		if (Vector2.Dot(dir, (Vector2.up + Vector2.left).normalized) > s_DiagAngCos)
		{
			return 6;
		}
		if (Vector2.Dot(dir, (Vector2.down + Vector2.left).normalized) > s_DiagAngCos)
		{
			return 7;
		}
		if (Vector2.Dot(dir, (Vector2.down + Vector2.right).normalized) > s_DiagAngCos)
		{
			return 4;
		}
		throw new ArgumentOutOfRangeException("dir", dir, null);
	}

	private static RotateDelegate GetRotator(int orientation)
	{
		return orientation switch
		{
			2 => (Vector2Int v) => v, 
			3 => (Vector2Int v) => new Vector2Int(-v.y, v.x), 
			0 => (Vector2Int v) => -v, 
			1 => (Vector2Int v) => new Vector2Int(v.y, -v.x), 
			5 => (Vector2Int v) => v, 
			6 => (Vector2Int v) => new Vector2Int(-v.y, v.x), 
			7 => (Vector2Int v) => -v, 
			4 => (Vector2Int v) => new Vector2Int(v.y, -v.x), 
			_ => throw new ArgumentOutOfRangeException("orientation", orientation, null), 
		};
	}

	private PatternGridData ConstructPattern(int direction)
	{
		HashSet<Vector2Int> hashSet = TempHashSet.Get<Vector2Int>();
		RotateDelegate rotator = GetRotator(direction);
		List<Vector2Int> obj = ((direction >= 4) ? DiagonalAlignedPattern.cells : AxisAlignedPattern.cells);
		bool flag = ExcludeFirstNode;
		foreach (Vector2Int item in obj)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				hashSet.Add(rotator(item));
			}
		}
		return PatternGridData.Create(hashSet, disposable: false);
	}
}
