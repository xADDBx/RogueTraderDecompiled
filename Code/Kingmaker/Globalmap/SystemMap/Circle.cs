using System;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

public class Circle
{
	public Vector2 Center;

	public float Radius;

	public Circle(Vector2 center, float radius)
	{
		Center = center;
		Radius = radius;
	}

	public bool Intersects(Vector2 startSegment, Vector2 endSegment)
	{
		float radius = Radius;
		float num = (startSegment.y - endSegment.y) / (startSegment.x - endSegment.x);
		float num2 = startSegment.y - num * startSegment.x + num * Center.x - Center.y;
		float num3 = -1f;
		if (num2 * num2 > radius * radius * (num * num + num3 * num3) + float.Epsilon)
		{
			return false;
		}
		if (Mathf.Abs(num2 * num2 - radius * radius * (num * num + num3 * num3)) < float.Epsilon)
		{
			return false;
		}
		float num4 = (0f - num) * num2 / (num * num + num3 * num3);
		float num5 = (0f - num3) * num2 / (num * num + num3 * num3);
		float num6 = Mathf.Sqrt((radius * radius - num2 * num2 / (num * num + num3 * num3)) / (num * num + num3 * num3));
		float num7 = num4 + num3 * num6 + Center.x;
		float num8 = num4 - num3 * num6 + Center.x;
		float num9 = num5 - num * num6 + Center.y;
		float num10 = num5 + num * num6 + Center.y;
		float magnitude = (startSegment - new Vector2(num7, num9)).magnitude;
		if ((startSegment - new Vector2(num8, num10)).magnitude < magnitude)
		{
			float num11 = num8;
			float num12 = num7;
			num7 = num11;
			num8 = num12;
			float num13 = num10;
			num12 = num9;
			num9 = num13;
			num10 = num12;
		}
		if ((startSegment.x - Center.x) * (startSegment.x - Center.x) + (startSegment.y - Center.y) * (startSegment.y - Center.y) <= radius * radius + float.Epsilon)
		{
			num7 = startSegment.x;
			num9 = startSegment.y;
		}
		if ((endSegment.x - Center.x - ShipPathHelper.Delta) * (endSegment.x - Center.x - ShipPathHelper.Delta) + (endSegment.y - Center.y - ShipPathHelper.Delta) * (endSegment.y - Center.y - ShipPathHelper.Delta) <= radius * radius + float.Epsilon)
		{
			num8 = endSegment.x;
			num10 = endSegment.y;
		}
		float num14 = Mathf.Min(startSegment.x, endSegment.x);
		float num15 = Mathf.Max(startSegment.x, endSegment.x);
		float num16 = Mathf.Min(startSegment.y, endSegment.y);
		float num17 = Mathf.Max(startSegment.y, endSegment.y);
		if (num14 - num7 <= float.Epsilon && num7 - num15 <= float.Epsilon && num14 - num8 <= float.Epsilon && num8 - num15 <= float.Epsilon && num16 - num9 <= float.Epsilon && num9 - num17 <= float.Epsilon && num16 - num10 <= float.Epsilon && num10 - num17 <= float.Epsilon)
		{
			return true;
		}
		return false;
	}

	public (Vector2 Tangent1, Vector2 Tangent2) TangentLinesFromPoint(Vector2 point)
	{
		float num = point.x - Center.x;
		float num2 = point.y - Center.y;
		float num3 = num * num + num2 * num2;
		if (num3 < Radius * Radius)
		{
			return (Tangent1: default(Vector2), Tangent2: default(Vector2));
		}
		float num4 = Mathf.Sqrt(num3);
		float num5 = Mathf.Atan2(num2, num);
		float num6 = Mathf.Acos(Radius / num4);
		float x = Center.x + Radius * Mathf.Cos(num5 + num6);
		float y = Center.y + Radius * Mathf.Sin(num5 + num6);
		float x2 = Center.x + Radius * Mathf.Cos(num5 - num6);
		return new ValueTuple<Vector2, Vector2>(item2: new Vector2(x2, Center.y + Radius * Mathf.Sin(num5 - num6)), item1: new Vector2(x, y));
	}
}
