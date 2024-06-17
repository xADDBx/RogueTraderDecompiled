using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Utility;

public class PolygonCutter
{
	private struct Point
	{
		public Vector2 Pos;

		public int OriginalIndex;
	}

	private readonly List<Vector3> m_Points;

	private List<int> m_Triangles = new List<int>();

	public List<int> TriangleIndices => m_Triangles;

	public PolygonCutter(IEnumerable<Vector3> points)
	{
		m_Points = points.ToList();
	}

	public bool Cut()
	{
		List<Point> list = m_Points.Select(delegate(Vector3 p, int i)
		{
			Point result = default(Point);
			result.Pos = new Vector2(p.x, p.z);
			result.OriginalIndex = i;
			return result;
		}).ToList();
		while (list.Count > 3)
		{
			int num = FindEar(list);
			if (num < 0)
			{
				return false;
			}
			int index = (num + list.Count - 1) % list.Count;
			int index2 = (num + list.Count + 1) % list.Count;
			m_Triangles.Add(list[index].OriginalIndex);
			m_Triangles.Add(list[num].OriginalIndex);
			m_Triangles.Add(list[index2].OriginalIndex);
			list.RemoveAt(num);
		}
		m_Triangles.Add(list[0].OriginalIndex);
		m_Triangles.Add(list[1].OriginalIndex);
		m_Triangles.Add(list[2].OriginalIndex);
		return true;
	}

	private int FindEar(List<Point> poly)
	{
		for (int i = 0; i < poly.Count; i++)
		{
			int num = (i + poly.Count - 1) % poly.Count;
			int num2 = (i + poly.Count + 1) % poly.Count;
			Vector2 pos = poly[num].Pos;
			Vector2 pos2 = poly[i].Pos;
			Vector2 pos3 = poly[num2].Pos;
			if (!VectorMath.RightOrColinear(pos, pos2, pos3))
			{
				continue;
			}
			bool flag = true;
			for (int j = 0; j < poly.Count; j++)
			{
				if (j != i && j != num2 && j != num && !IsConvex(poly, j) && IsPointInsideTriangle(pos, pos2, pos3, poly[j].Pos))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool IsPointInsideTriangle(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 point)
	{
		bool num = VectorMath.RightOrColinear(p0, p1, point);
		bool flag = VectorMath.RightOrColinear(p1, p2, point);
		bool flag2 = VectorMath.RightOrColinear(p2, p0, point);
		if (num == flag)
		{
			return flag == flag2;
		}
		return false;
	}

	private bool IsConvex(List<Point> poly, int vertexIdx)
	{
		Vector2 pos = poly[(vertexIdx + poly.Count - 1) % poly.Count].Pos;
		Vector2 pos2 = poly[vertexIdx].Pos;
		Vector2 pos3 = poly[(vertexIdx + poly.Count + 1) % poly.Count].Pos;
		return VectorMath.RightOrColinear(pos, pos2, pos3);
	}
}
