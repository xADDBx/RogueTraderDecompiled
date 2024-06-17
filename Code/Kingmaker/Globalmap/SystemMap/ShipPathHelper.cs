using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

public static class ShipPathHelper
{
	public static float Delta = 1f;

	private static float Epsilon = float.Epsilon;

	public static Vector3 LandingPoint(StarSystemObjectView sso)
	{
		SphereCollider component = sso.gameObject.GetComponent<SphereCollider>();
		float num = ((component != null) ? (component.radius + Delta) : Delta);
		return sso.gameObject.transform.position + num * Vector2.down.To3D();
	}

	public static List<Vector3> FindPath(Vector3 from, Vector3 to)
	{
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode != GameModeType.StarSystem)
		{
			return new List<Vector3>();
		}
		List<Circle> list = new List<Circle>();
		foreach (StarSystemObjectEntity starSystemObject in Game.Instance.State.StarSystemObjects)
		{
			if (!(starSystemObject is AnomalyEntityData))
			{
				float num = starSystemObject.View.GetComponentNonAlloc<SphereCollider>()?.radius ?? 0f;
				list.Add(new Circle(starSystemObject.Position.To2D(), num + Delta));
				if ((starSystemObject.Position.To2D() - to.To2D()).magnitude <= num + Delta + Epsilon)
				{
					to = starSystemObject.Position + (to - starSystemObject.Position).normalized * (num + Delta);
				}
			}
		}
		List<PathSegment> list2 = CalculatePath(from.To2D(), to.To2D(), list);
		List<Vector3> list3 = new List<Vector3> { from };
		foreach (PathSegment item in list2)
		{
			if (item is LinePathSegment linePathSegment)
			{
				List<Vector3> list4 = list3;
				if ((list4[list4.Count - 1] - linePathSegment.Start.To3D()).magnitude > Epsilon && (linePathSegment.Start - linePathSegment.End).magnitude > Epsilon)
				{
					list3.Add(linePathSegment.Start.To3D());
				}
				if (!list3.Empty())
				{
					List<Vector3> list5 = list3;
					if (Mathf.Abs((list5[list5.Count - 1].To2D() - linePathSegment.End).magnitude) > Epsilon)
					{
						list3.Add(linePathSegment.End.To3D());
					}
				}
			}
			if (item is ArcPathSegment arcPathSegment)
			{
				List<Vector3> list6 = (from poi in GetPointsOnShortestArc(arcPathSegment.Circle, arcPathSegment.AngleStart, arcPathSegment.AngleEnd, 20)
					select poi.To3D()).ToList();
				Vector3 vector = list6[0];
				List<Vector3> list7 = list3;
				float magnitude = (vector - list7[list7.Count - 1]).magnitude;
				Vector3 vector2 = list6[list6.Count - 1];
				List<Vector3> list8 = list3;
				if (magnitude > (vector2 - list8[list8.Count - 1]).magnitude)
				{
					list6.Reverse();
				}
				list3 = list3.Concat(list6).ToList();
			}
		}
		List<Vector3> list9 = list3;
		if ((list9[list9.Count - 1] - to).magnitude > Epsilon)
		{
			list3.Add(to);
		}
		List<Vector3> list10 = new List<Vector3>();
		list10.Add(list3[0]);
		for (int i = 1; i < list3.Count; i++)
		{
			if (Mathf.Abs((list3[i].To2D() - list3[i - 1].To2D()).magnitude) > Epsilon)
			{
				list10.Add(list3[i]);
			}
		}
		return list10;
	}

	private static float Vector2Cross(Vector2 vec1, Vector2 vec2)
	{
		return vec1.x * vec2.y - vec1.y * vec2.x;
	}

	private static float Angle(Vector2 vec)
	{
		return Mathf.Atan2(vec.y, vec.x);
	}

	private static float AngleOnCircle(Vector2 vec, Circle circle)
	{
		return Angle(vec - circle.Center);
	}

	private static List<PathSegment> CalculatePath(Vector2 start, Vector2 end, List<Circle> starSystemObjects)
	{
		List<PathSegment> list = new List<PathSegment>();
		List<int> excludedIndexes = new List<int>();
		int? currentCircleIndex = null;
		Circle circle = null;
		foreach (Circle starSystemObject in starSystemObjects)
		{
			if ((starSystemObject.Center - start).magnitude <= starSystemObject.Radius + Delta + Epsilon)
			{
				circle = starSystemObject;
			}
		}
		if (circle != null)
		{
			int num = starSystemObjects.IndexOf(circle);
			excludedIndexes.Add(num);
			currentCircleIndex = num;
		}
		LinePathSegment linePathSegment = null;
		if ((circle != null && !circle.Intersects(start, end)) || circle == null)
		{
			int? num2;
			(linePathSegment, num2) = FindBestFreeTangentialOrPath(start, end, starSystemObjects, currentCircleIndex, ref excludedIndexes);
			list.Add(linePathSegment);
			currentCircleIndex = num2;
		}
		if (currentCircleIndex.HasValue)
		{
			list = list.Concat(FindPathFromCircle(linePathSegment?.End ?? start, end, starSystemObjects, currentCircleIndex.Value, null, ref excludedIndexes)).ToList();
		}
		return list;
	}

	private static (LinePathSegment, int?) FindBestFreeTangentialOrPath(Vector2 start, Vector2 end, List<Circle> starSystemObjects, int? currentCircleIndex, ref List<int> excludedIndexes)
	{
		int? closestIntersectingCircle = GetClosestIntersectingCircle(start, end, starSystemObjects, excludedIndexes);
		if (!closestIntersectingCircle.HasValue || (currentCircleIndex.HasValue && closestIntersectingCircle.Value == currentCircleIndex.Value))
		{
			return (new LinePathSegment(start, end), null);
		}
		excludedIndexes.Add(closestIntersectingCircle.Value);
		Circle circle = starSystemObjects[closestIntersectingCircle.Value];
		(Vector2 Tangent1, Vector2 Tangent2) tuple = circle.TangentLinesFromPoint(start);
		Vector2 item = tuple.Tangent1;
		Vector2 item2 = tuple.Tangent2;
		Vector2 vec = end - start;
		Vector2 vec2 = circle.Center - start;
		float num = Vector2Cross(vec, vec2);
		float num2 = Vector2Cross(item, vec2) * num;
		float num3 = Vector2Cross(item2, vec2) * num;
		Vector2 end2;
		if (num2 * num3 < 0f)
		{
			end2 = ((num2 > 0f) ? item : item2);
		}
		else
		{
			float num4 = item.magnitude + (item - start).magnitude;
			float num5 = item2.magnitude + (item2 - start).magnitude;
			end2 = ((num4 < num5) ? item : item2);
		}
		return (new LinePathSegment(start, end2), closestIntersectingCircle);
	}

	private static List<PathSegment> FindPathFromCircle(Vector2 start, Vector2 end, List<Circle> starSystemObjects, int c1, int? c2, ref List<int> excludedIndexes)
	{
		List<PathSegment> list = new List<PathSegment>();
		Vector2 vec = end - start;
		Circle circle = starSystemObjects[c1];
		var (vector, vector2) = circle.TangentLinesFromPoint(end);
		if (vector == default(Vector2) && vector2 == default(Vector2))
		{
			float angleStart = AngleOnCircle(start, circle);
			float angleEnd = AngleOnCircle(end, circle);
			excludedIndexes.Add(c1);
			list.Add(new ArcPathSegment(circle, angleStart, angleEnd));
			return list;
		}
		float angleStart2 = AngleOnCircle(start, circle);
		Vector2 vec2 = circle.Center - end;
		float num = Vector2Cross(vec, vec2);
		Vector2 vector3 = ((Vector2Cross(circle.Center - vector, vec2) * num > 0f) ? vector : vector2);
		excludedIndexes.Add(c1);
		if (c2.HasValue)
		{
			excludedIndexes.Add(c2.Value);
		}
		int? closestIntersectingCircle = GetClosestIntersectingCircle(vector3, end, starSystemObjects, excludedIndexes);
		if (!closestIntersectingCircle.HasValue)
		{
			float angleEnd2 = AngleOnCircle(vector3, circle);
			list.Add(new ArcPathSegment(circle, angleStart2, angleEnd2));
			list.Add(new LinePathSegment(vector3, end));
		}
		else
		{
			Circle circle2 = starSystemObjects[closestIntersectingCircle.Value];
			List<LinePathSegment> list2 = CommonCirclesTangents(circle, circle2);
			Vector2 vec3 = circle.Center - circle2.Center;
			float num2 = Vector2Cross(start - circle.Center, vec3);
			float num3 = float.MaxValue;
			LinePathSegment linePathSegment = null;
			foreach (LinePathSegment item in list2)
			{
				if (!(Vector2Cross(item.Start - circle.Center, vec3) * num2 < 0f))
				{
					float num4 = (item.End - item.Start).magnitude + (end - item.End).magnitude;
					if (num3 > num4)
					{
						num3 = num4;
						linePathSegment = item;
					}
				}
			}
			if (linePathSegment == null)
			{
				return list;
			}
			list.Add(new ArcPathSegment(circle, angleStart2, AngleOnCircle(linePathSegment.Start, circle)));
			list.Add(new LinePathSegment(linePathSegment.Start, linePathSegment.End));
			list = list.Concat(FindPathFromCircle(linePathSegment.End, end, starSystemObjects, closestIntersectingCircle.Value, null, ref excludedIndexes)).ToList();
		}
		return list;
	}

	private static int? GetClosestIntersectingCircle(Vector2 start, Vector2 end, List<Circle> circles, List<int> excluded)
	{
		int? result = null;
		float num = float.MaxValue;
		for (int i = 0; i < circles.Count; i++)
		{
			if (!excluded.Contains(i) && circles[i].Intersects(start, end))
			{
				float num2 = Mathf.Max((circles[i].Center - start).magnitude - circles[i].Radius, 0f);
				if (num2 < num)
				{
					num = num2;
					result = i;
				}
			}
		}
		return result;
	}

	private static List<LinePathSegment> CommonCirclesTangents(Circle c1, Circle c2)
	{
		List<LinePathSegment> list = new List<LinePathSegment>();
		Vector2 vector = c1.Center - c2.Center;
		float magnitude = vector.magnitude;
		if (magnitude * magnitude <= (c1.Radius - c2.Radius) * (c1.Radius - c2.Radius))
		{
			return list;
		}
		Vector2 vector2 = -vector.normalized;
		for (int num = 1; num >= -1; num -= 2)
		{
			float num2 = (c1.Radius - (float)num * c2.Radius) / magnitude;
			float num3 = Mathf.Sqrt(Mathf.Max(0f, 1f - num2 * num2));
			for (int num4 = 1; num4 >= -1; num4 -= 2)
			{
				Vector2 vector3 = new Vector2(vector2.x * num2 - (float)num4 * num3 * vector2.y, vector2.y * num2 + (float)num4 * num3 * vector2.x);
				Vector2 start = c1.Center + vector3 * c1.Radius;
				Vector2 end = c2.Center + vector3 * c2.Radius * num;
				list.Add(new LinePathSegment(start, end));
			}
		}
		return list;
	}

	public static List<Vector2> GetPointsOnShortestArc(Circle circle, float startAngle, float endAngle, int numPoints)
	{
		List<Vector2> list = new List<Vector2>();
		int num = 1;
		float num2 = startAngle * 57.29578f;
		float num3 = endAngle * 57.29578f;
		for (; num2 < 0f; num2 += 360f)
		{
		}
		while (num2 > 360f)
		{
			num2 -= 360f;
		}
		for (; num3 < 0f; num3 += 360f)
		{
		}
		while (num3 > 360f)
		{
			num3 -= 360f;
		}
		float num4 = num3 - num2;
		if (num4 > 180f)
		{
			num4 = 360f - num4;
			num = -1;
		}
		if (num4 < -180f)
		{
			num4 = -360f - num4;
			num = -1;
		}
		float num5 = num4 / (float)(numPoints - 1);
		for (int i = 0; i < numPoints; i++)
		{
			float num6 = num2 + (float)i * num5 * (float)num;
			float x = circle.Center.x + circle.Radius * Mathf.Cos(num6 * (MathF.PI / 180f));
			float y = circle.Center.y + circle.Radius * Mathf.Sin(num6 * (MathF.PI / 180f));
			list.Add(new Vector2(x, y));
		}
		return list;
	}
}
