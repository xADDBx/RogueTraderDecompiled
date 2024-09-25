using System;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public static class CustomGraphHelper
{
	public static readonly int[] LeftNeighbourDirection = new int[8] { 4, 5, 6, 7, 1, 2, 3, 0 };

	public static readonly int[] RightNeighbourDirection = new int[8] { 7, 4, 5, 6, 0, 1, 2, 3 };

	public static readonly int[] OppositeDirections = new int[8] { 2, 3, 0, 1, 6, 7, 4, 5 };

	private static readonly float Cos45div2 = Mathf.Cos(MathF.PI / 8f);

	private static readonly float CosRest = 1f - Cos45div2;

	public static int GetWarhammerCellDistance(CustomGridNodeBase a, CustomGridNodeBase b)
	{
		return GetWarhammerLength(new Vector2Int(a.XCoordinateInGrid - b.XCoordinateInGrid, a.ZCoordinateInGrid - b.ZCoordinateInGrid));
	}

	public static int GetWarhammerCellDistance(Vector2Int a, Vector2Int b)
	{
		return GetWarhammerLength(a - b);
	}

	public static int GetWarhammerLength(Vector2Int v)
	{
		int val = Math.Abs(v.y);
		int val2 = Math.Abs(v.x);
		int num = Math.Max(val2, val);
		int num2 = Math.Min(val2, val);
		return num + num2 / 2;
	}

	public static int CellDistanceTo(this CustomGridNodeBase a, CustomGridNodeBase b)
	{
		return GetWarhammerCellDistance(a, b);
	}

	public static int GetOrientationFromDirection(int dir)
	{
		return dir switch
		{
			0 => 180, 
			1 => 90, 
			2 => 0, 
			3 => 270, 
			4 => 135, 
			5 => 45, 
			6 => 315, 
			7 => 225, 
			_ => 0, 
		};
	}

	public static Vector3 GetVector3Direction(int dir)
	{
		return Quaternion.AngleAxis(GetOrientationFromDirection(dir), Vector3.up) * Vector3.forward;
	}

	public static WarhammerCombatSide GetWarhammerAttackSide(Vector3 targetFwd, Vector3 casterAttack, Size targetSize)
	{
		float num = Vector2.SignedAngle(new Vector2(targetFwd.x, targetFwd.z), new Vector2(casterAttack.x, casterAttack.z));
		WarhammerCombatSide result = WarhammerCombatSide.Front;
		float[] cornerDegreesSquareUnits = SizePathfindingHelper.GetCornerDegreesSquareUnits(targetSize);
		if (num < cornerDegreesSquareUnits[0] || num > cornerDegreesSquareUnits[3])
		{
			result = WarhammerCombatSide.Front;
		}
		if (num > cornerDegreesSquareUnits[1] && num < cornerDegreesSquareUnits[2])
		{
			result = WarhammerCombatSide.Back;
		}
		if (num > cornerDegreesSquareUnits[0] && num < cornerDegreesSquareUnits[1])
		{
			result = WarhammerCombatSide.Left;
		}
		if (num > cornerDegreesSquareUnits[2] && num < cornerDegreesSquareUnits[3])
		{
			result = WarhammerCombatSide.Right;
		}
		return result;
	}

	public static int GuessDirection(Vector3 fwd)
	{
		fwd = fwd.ToXZ();
		float magnitude = fwd.magnitude;
		if ((double)magnitude < 0.1)
		{
			throw new ArgumentException($"Invalid value {fwd}", "fwd");
		}
		fwd /= magnitude;
		float num = Vector3.Dot(fwd, Vector3.right);
		float num2 = Vector3.Dot(fwd, Vector3.forward);
		if (num > Cos45div2)
		{
			return 1;
		}
		if (num < 0f - Cos45div2)
		{
			return 3;
		}
		if (num2 > Cos45div2)
		{
			return 2;
		}
		if (num2 < 0f - Cos45div2)
		{
			return 0;
		}
		if (num > CosRest && num2 > CosRest)
		{
			return 5;
		}
		if (num < 0f - CosRest && num2 > CosRest)
		{
			return 6;
		}
		if (num < 0f - CosRest && num2 < 0f - CosRest)
		{
			return 7;
		}
		if (num > CosRest && num2 < 0f - CosRest)
		{
			return 4;
		}
		throw new Exception($"GuessDirection: internal error, report to programmers (fwd == {fwd})");
	}

	public static Vector3 AdjustDirection(Vector3 dir)
	{
		if (dir.sqrMagnitude > 1.1f || dir.sqrMagnitude < 0.9f)
		{
			throw new ArgumentException("Need nonzero vector", "dir");
		}
		float num = Vector3.Dot(dir, Vector3.right);
		float num2 = Vector3.Dot(dir, Vector3.forward);
		if (num > Cos45div2)
		{
			return Vector3.right;
		}
		if (num < 0f - Cos45div2)
		{
			return Vector3.left;
		}
		if (num2 > Cos45div2)
		{
			return Vector3.forward;
		}
		if (num2 < 0f - Cos45div2)
		{
			return Vector3.back;
		}
		if (num > CosRest && num2 > CosRest)
		{
			return (Vector3.right + Vector3.forward).normalized;
		}
		if (num < 0f - CosRest && num2 > CosRest)
		{
			return (Vector3.left + Vector3.forward).normalized;
		}
		if (num < 0f - CosRest && num2 < 0f - CosRest)
		{
			return (Vector3.left + Vector3.back).normalized;
		}
		if (num > CosRest && num2 < 0f - CosRest)
		{
			return (Vector3.right + Vector3.back).normalized;
		}
		return Vector3.forward;
	}

	public static Vector3 ToCellSizedVector(this Vector3 v)
	{
		Vector3 result = AdjustDirection(v);
		result = new Vector3((Math.Abs(result.x) < 0.0001f) ? 0f : ((float)Math.Sign(result.x) * GraphParamsMechanicsCache.GridCellSize), 0f, (Math.Abs(result.z) < 0.0001f) ? 0f : ((float)Math.Sign(result.z) * GraphParamsMechanicsCache.GridCellSize));
		return result;
	}

	public static bool HasConnectionBetweenNodes(CustomGridNodeBase nodeFrom, CustomGridNodeBase nodeTo)
	{
		int num = nodeTo.XCoordinateInGrid - nodeFrom.XCoordinateInGrid;
		int num2 = nodeTo.ZCoordinateInGrid - nodeFrom.ZCoordinateInGrid;
		int num3 = 0;
		num3 = ((num > 0) ? ((num2 > 0) ? 5 : ((num2 >= 0) ? 1 : 4)) : ((num >= 0) ? ((num2 > 0) ? 2 : 0) : ((num2 > 0) ? 6 : ((num2 < 0) ? 7 : 3))));
		return ((CustomGridNode)nodeFrom).HasConnectionInDirection(num3);
	}
}
