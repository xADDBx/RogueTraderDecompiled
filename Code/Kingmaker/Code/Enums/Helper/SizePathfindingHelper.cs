using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Pathfinding;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Code.Enums.Helper;

public static class SizePathfindingHelper
{
	public static IntRect GetRectForSize(Size size)
	{
		return size switch
		{
			Size.Fine => new IntRect(0, 0, 0, 0), 
			Size.Diminutive => new IntRect(0, 0, 0, 0), 
			Size.Tiny => new IntRect(0, 0, 0, 0), 
			Size.Small => new IntRect(0, 0, 0, 0), 
			Size.Medium => new IntRect(0, 0, 0, 0), 
			Size.Large => new IntRect(0, 0, 1, 1), 
			Size.Huge => new IntRect(-1, -1, 1, 1), 
			Size.Gargantuan => new IntRect(-1, -1, 2, 2), 
			Size.Colossal => new IntRect(-2, -2, 2, 2), 
			Size.Raider_1x1 => new IntRect(0, 0, 0, 0), 
			Size.Frigate_1x2 => new IntRect(0, 0, 0, 1), 
			Size.Cruiser_2x4 => new IntRect(0, 0, 1, 3), 
			Size.GrandCruiser_3x6 => new IntRect(0, 0, 2, 5), 
			_ => throw new ArgumentOutOfRangeException("size", size, null), 
		};
	}

	public static float[] GetCornerDegreesSquareUnits(Size size)
	{
		_ = 9;
		return new float[4] { -135f, -45f, 45f, 135f };
	}

	public static IntRect GetSizeRect(this MechanicEntity entity)
	{
		return GetRectForSize(entity.Size);
	}

	public static Vector3 GetSizePositionOffset(MechanicEntity entity)
	{
		return GetSizePositionOffset(entity, entity.IsInCombat);
	}

	public static Vector3 GetSizePositionOffset(MechanicEntity entity, bool inBattle)
	{
		if (!(entity is StarshipEntity))
		{
			if (!inBattle)
			{
				return Vector3.zero;
			}
			return GetSizePositionOffsetForGroundUnit(entity);
		}
		return GetSizePositionOffset(entity.SizeRect, entity.Forward);
	}

	private static Vector3 GetSizePositionOffsetForGroundUnit(MechanicEntity entity)
	{
		return GetSizePositionOffsetForGroundUnit(entity.Size).To3D();
	}

	private static Vector2 GetSizePositionOffsetForGroundUnit(Size size)
	{
		return size switch
		{
			Size.Fine => Vector2.zero, 
			Size.Diminutive => Vector2.zero, 
			Size.Tiny => Vector2.zero, 
			Size.Small => Vector2.zero, 
			Size.Medium => Vector2.zero, 
			Size.Large => Vector2.one * GraphParamsMechanicsCache.GridCellSize / 2f, 
			Size.Huge => Vector2.zero, 
			Size.Gargantuan => Vector2.one * GraphParamsMechanicsCache.GridCellSize / 2f, 
			Size.Colossal => Vector2.zero, 
			Size.Raider_1x1 => Vector2.zero, 
			Size.Frigate_1x2 => Vector2.up * GraphParamsMechanicsCache.GridCellSize / 2f, 
			Size.Cruiser_2x4 => new Vector2(GraphParamsMechanicsCache.GridCellSize, GraphParamsMechanicsCache.GridCellSize * 3f / 2f), 
			Size.GrandCruiser_3x6 => new Vector2(GraphParamsMechanicsCache.GridCellSize * 3f / 2f, GraphParamsMechanicsCache.GridCellSize * 6f / 2f), 
			_ => throw new ArgumentOutOfRangeException("size", size, null), 
		};
	}

	public static Vector3 GetSizePositionOffset(IntRect size)
	{
		return new Vector3(((float)size.xmin + (float)(size.xmax - size.xmin) / 2f) * GraphParamsMechanicsCache.GridCellSize, 0f, ((float)size.ymin + (float)(size.ymax - size.ymin) / 2f) * GraphParamsMechanicsCache.GridCellSize);
	}

	public static Vector3 GetSizePositionOffset(IntRect size, Vector3 direction, bool shiftRight = true)
	{
		Vector3 result = new Vector3(0f, 0f, 0f);
		if (size.Height == size.Width)
		{
			if (size.Height > 1)
			{
				float num = GraphParamsMechanicsCache.GridCellSize / 2f;
				result.x += num;
				result.z += num;
				return result;
			}
			return result;
		}
		float num2 = GraphParamsMechanicsCache.GridCellSize * -0.5f;
		Vector3 vector = default(Vector3);
		vector.x = (float)Math.Sign(direction.x) * num2;
		vector.y = 0f;
		vector.z = (float)Math.Sign(direction.z) * num2;
		result = vector;
		if (Math.Abs(direction.x) > Math.Abs(direction.z))
		{
			result.z *= Math.Abs(direction.z / direction.x);
		}
		else
		{
			result.x *= Math.Abs(direction.x / direction.z);
		}
		float num3 = Mathf.Clamp01(Mathf.Max(Math.Abs(direction.x), Mathf.Abs(direction.z)) - 0.707f) / 0.29299998f;
		Vector3 vector2 = Quaternion.AngleAxis(90f, Vector3.up) * direction.normalized;
		return result - vector2 * ((float)(1 - size.Width % 2) * num2 * num3);
	}

	public static int GetLesserSide(this Size size)
	{
		IntRect rectForSize = GetRectForSize(size);
		return Math.Min(rectForSize.Height, rectForSize.Width);
	}

	public static Vector3 FromMechanicsToViewPosition(MechanicEntity entity, Vector3 mechanicsPosition)
	{
		return FromMechanicsToViewPosition(entity, mechanicsPosition, entity.IsInCombat);
	}

	public static Vector3 FromMechanicsToViewPosition(MechanicEntity entity, Vector3 mechanicsPosition, bool inBattle)
	{
		Vector3 sizePositionOffset = GetSizePositionOffset(entity, inBattle);
		return mechanicsPosition + sizePositionOffset;
	}

	public static Vector3 FromViewToMechanicsPosition(MechanicEntity entity, Vector3 mechanicsPosition)
	{
		return FromViewToMechanicsPosition(entity, mechanicsPosition, entity.IsInCombat);
	}

	public static Vector3 FromViewToMechanicsPosition(MechanicEntity entity, Vector3 viewPosition, bool inBattle)
	{
		Vector3 sizePositionOffset = GetSizePositionOffset(entity, inBattle);
		return viewPosition - sizePositionOffset;
	}

	public static bool IsCloserThanHalfCell(MechanicEntity unit, Vector3 point)
	{
		return (FromMechanicsToViewPosition(unit, unit.Position) - point).ToXZ().magnitude < 0.675f;
	}

	public static int BoundsDistance(IntRect leftRect, Vector2Int leftPos, IntRect rightRect, Vector2Int rightPos)
	{
		UnitRect unitRect = new UnitRect(leftPos, leftRect);
		UnitRect unitRect2 = new UnitRect(rightPos, rightRect);
		bool flag = unitRect.Left > unitRect2.Right;
		bool flag2 = unitRect.Right < unitRect2.Left;
		bool flag3 = unitRect.Top < unitRect2.Bottom;
		bool flag4 = unitRect.Bottom > unitRect2.Top;
		if (flag3 && flag)
		{
			return Mathf.Max(unitRect2.Bottom - unitRect.Top - 1, unitRect.Left - unitRect2.Right - 1);
		}
		if (flag3 && flag2)
		{
			return Mathf.Max(unitRect2.Bottom - unitRect.Top - 1, unitRect2.Left - unitRect.Right - 1);
		}
		if (flag4 && flag)
		{
			return Mathf.Max(unitRect.Bottom - unitRect2.Top - 1, unitRect.Left - unitRect2.Right - 1);
		}
		if (flag4 && flag2)
		{
			return Mathf.Max(unitRect.Bottom - unitRect2.Top - 1, unitRect2.Left - unitRect.Right - 1);
		}
		if (flag)
		{
			return unitRect.Left - unitRect2.Right - 1;
		}
		if (flag2)
		{
			return unitRect2.Left - unitRect.Right - 1;
		}
		if (flag3)
		{
			return unitRect2.Bottom - unitRect.Top - 1;
		}
		if (flag4)
		{
			return unitRect.Bottom - unitRect2.Top - 1;
		}
		return 0;
	}

	public static float BoundsDistance(BaseUnitEntity leftUnit, BaseUnitEntity rightUnit)
	{
		float num = GraphParamsMechanicsCache.GridCellSize / 2f;
		IntRect sizeRect = leftUnit.SizeRect;
		IntRect sizeRect2 = rightUnit.SizeRect;
		Vector3 vector = (leftUnit.Position + Vector3.one * num) / GraphParamsMechanicsCache.GridCellSize;
		Vector3 vector2 = (rightUnit.Position + Vector3.one * num) / GraphParamsMechanicsCache.GridCellSize;
		Vector2Int leftPos = new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.z)) - Vector2Int.one;
		Vector2Int rightPos = new Vector2Int(Mathf.RoundToInt(vector2.x), Mathf.RoundToInt(vector2.z)) - Vector2Int.one;
		return BoundsDistance(sizeRect, leftPos, sizeRect2, rightPos);
	}
}
