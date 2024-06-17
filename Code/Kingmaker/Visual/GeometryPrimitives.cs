using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual;

public static class GeometryPrimitives
{
	private struct TriangleIndices
	{
		public int v1;

		public int v2;

		public int v3;

		public TriangleIndices(int v1, int v2, int v3)
		{
			this.v1 = v1;
			this.v2 = v2;
			this.v3 = v3;
		}
	}

	private static int GetMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache, float radius)
	{
		bool num = p1 < p2;
		long num2 = (num ? p1 : p2);
		long num3 = (num ? p2 : p1);
		long key = (num2 << 32) + num3;
		if (cache.TryGetValue(key, out var value))
		{
			return value;
		}
		Vector3 vector = vertices[p1];
		Vector3 vector2 = vertices[p2];
		Vector3 vector3 = new Vector3((vector.x + vector2.x) / 2f, (vector.y + vector2.y) / 2f, (vector.z + vector2.z) / 2f);
		int count = vertices.Count;
		vertices.Add(vector3.normalized * radius);
		cache.Add(key, count);
		return count;
	}

	public static Mesh CreateIcoSphere(float radius, int recursionLevel = 3, bool outerRadius = false)
	{
		Mesh mesh = new Mesh
		{
			name = "IcoSphere"
		};
		List<Vector3> vertices = new List<Vector3>();
		Dictionary<long, int> cache = new Dictionary<long, int>();
		float num = (1f + Mathf.Sqrt(5f)) / 2f;
		vertices.Add(new Vector3(-1f, num, 0f).normalized * radius);
		vertices.Add(new Vector3(1f, num, 0f).normalized * radius);
		vertices.Add(new Vector3(-1f, 0f - num, 0f).normalized * radius);
		vertices.Add(new Vector3(1f, 0f - num, 0f).normalized * radius);
		vertices.Add(new Vector3(0f, -1f, num).normalized * radius);
		vertices.Add(new Vector3(0f, 1f, num).normalized * radius);
		vertices.Add(new Vector3(0f, -1f, 0f - num).normalized * radius);
		vertices.Add(new Vector3(0f, 1f, 0f - num).normalized * radius);
		vertices.Add(new Vector3(num, 0f, -1f).normalized * radius);
		vertices.Add(new Vector3(num, 0f, 1f).normalized * radius);
		vertices.Add(new Vector3(0f - num, 0f, -1f).normalized * radius);
		vertices.Add(new Vector3(0f - num, 0f, 1f).normalized * radius);
		float num2 = 1f;
		if (outerRadius)
		{
			Vector3 vector = vertices[0] + vertices[1] + vertices[5];
			num2 = radius / (vector / 3f).magnitude;
		}
		List<TriangleIndices> list = new List<TriangleIndices>();
		list.Add(new TriangleIndices(0, 11, 5));
		list.Add(new TriangleIndices(0, 5, 1));
		list.Add(new TriangleIndices(0, 1, 7));
		list.Add(new TriangleIndices(0, 7, 10));
		list.Add(new TriangleIndices(0, 10, 11));
		list.Add(new TriangleIndices(1, 5, 9));
		list.Add(new TriangleIndices(5, 11, 4));
		list.Add(new TriangleIndices(11, 10, 2));
		list.Add(new TriangleIndices(10, 7, 6));
		list.Add(new TriangleIndices(7, 1, 8));
		list.Add(new TriangleIndices(3, 9, 4));
		list.Add(new TriangleIndices(3, 4, 2));
		list.Add(new TriangleIndices(3, 2, 6));
		list.Add(new TriangleIndices(3, 6, 8));
		list.Add(new TriangleIndices(3, 8, 9));
		list.Add(new TriangleIndices(4, 9, 5));
		list.Add(new TriangleIndices(2, 4, 11));
		list.Add(new TriangleIndices(6, 2, 10));
		list.Add(new TriangleIndices(8, 6, 7));
		list.Add(new TriangleIndices(9, 8, 1));
		for (int i = 0; i < recursionLevel; i++)
		{
			List<TriangleIndices> list2 = new List<TriangleIndices>();
			foreach (TriangleIndices item in list)
			{
				int middlePoint = GetMiddlePoint(item.v1, item.v2, ref vertices, ref cache, radius);
				int middlePoint2 = GetMiddlePoint(item.v2, item.v3, ref vertices, ref cache, radius);
				int middlePoint3 = GetMiddlePoint(item.v3, item.v1, ref vertices, ref cache, radius);
				list2.Add(new TriangleIndices(item.v1, middlePoint, middlePoint3));
				list2.Add(new TriangleIndices(item.v2, middlePoint2, middlePoint));
				list2.Add(new TriangleIndices(item.v3, middlePoint3, middlePoint2));
				list2.Add(new TriangleIndices(middlePoint, middlePoint2, middlePoint3));
			}
			list = list2;
		}
		if (outerRadius)
		{
			for (int j = 0; j < vertices.Count; j++)
			{
				vertices[j] *= num2;
			}
		}
		mesh.vertices = vertices.ToArray();
		List<int> list3 = new List<int>();
		for (int k = 0; k < list.Count; k++)
		{
			list3.Add(list[k].v1);
			list3.Add(list[k].v2);
			list3.Add(list[k].v3);
		}
		mesh.triangles = list3.ToArray();
		mesh.uv = new Vector2[vertices.Count];
		Vector3[] array = new Vector3[vertices.Count];
		for (int l = 0; l < array.Length; l++)
		{
			array[l] = vertices[l].normalized;
		}
		mesh.normals = array;
		mesh.RecalculateBounds();
		return mesh;
	}
}
