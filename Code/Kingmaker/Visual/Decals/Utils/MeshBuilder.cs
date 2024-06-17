using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.Decals.Utils;

public class MeshBuilder
{
	private readonly List<Vector3> m_Vertices = new List<Vector3>();

	private readonly List<Vector3> m_Normals = new List<Vector3>();

	private readonly List<Vector3> m_PushNormals = new List<Vector3>();

	private readonly List<Vector2> m_TexCoords = new List<Vector2>();

	private readonly List<Vector2> m_LmapUv = new List<Vector2>();

	private readonly List<int> m_Indices = new List<int>();

	public void AddPolygon(List<Vector3> poly, List<Vector2> lmapUv, List<Vector3> normals, Vector3 pushNormal, Rect uvRect)
	{
		if (lmapUv.Count > 0)
		{
			int item = AddVertex(poly[0], lmapUv[0], normals[0], pushNormal, uvRect);
			for (int i = 1; i < poly.Count - 1; i++)
			{
				int item2 = AddVertex(poly[i], lmapUv[i], normals[i], pushNormal, uvRect);
				int item3 = AddVertex(poly[i + 1], lmapUv[i + 1], normals[i + 1], pushNormal, uvRect);
				m_Indices.Add(item);
				m_Indices.Add(item2);
				m_Indices.Add(item3);
			}
		}
		else
		{
			int item4 = AddVertex(poly[0], normals[0], pushNormal, uvRect);
			for (int j = 1; j < poly.Count - 1; j++)
			{
				int item5 = AddVertex(poly[j], normals[j], pushNormal, uvRect);
				int item6 = AddVertex(poly[j + 1], normals[j], pushNormal, uvRect);
				m_Indices.Add(item4);
				m_Indices.Add(item5);
				m_Indices.Add(item6);
			}
		}
	}

	private int AddVertex(Vector3 vertex, Vector3 normal, Vector3 pushNormal, Rect uvRect)
	{
		int num = FindVertex(vertex, normal);
		if (num == -1)
		{
			m_Vertices.Add(vertex);
			m_Normals.Add(normal);
			m_PushNormals.Add(pushNormal);
			AddTexCoord(vertex, uvRect);
			return m_Vertices.Count - 1;
		}
		m_Normals[num] = (m_Normals[num] + normal).normalized;
		m_PushNormals[num] = (m_PushNormals[num] + pushNormal).normalized;
		return num;
	}

	private int AddVertex(Vector3 vertex, Vector2 lmapUv, Vector3 normal, Vector3 pushNormal, Rect uvRect)
	{
		int num = FindVertex(vertex, normal, lmapUv);
		if (num == -1)
		{
			m_Vertices.Add(vertex);
			m_LmapUv.Add(lmapUv);
			m_Normals.Add(normal);
			m_PushNormals.Add(pushNormal);
			AddTexCoord(vertex, uvRect);
			return m_Vertices.Count - 1;
		}
		m_PushNormals[num] = (m_PushNormals[num] + pushNormal).normalized;
		return num;
	}

	private int FindVertex(Vector3 vertex, Vector3 normal)
	{
		for (int i = 0; i < m_Vertices.Count; i++)
		{
			Vector3 vector = m_Vertices[i];
			Vector3 vector2 = m_Normals[i];
			if (Mathf.Approximately(vector.x, vertex.x) && Mathf.Approximately(vector.y, vertex.y) && Mathf.Approximately(vector.z, vertex.z) && Mathf.Approximately(vector2.x, normal.x) && Mathf.Approximately(vector2.y, normal.y) && Mathf.Approximately(vector2.z, normal.z))
			{
				return i;
			}
		}
		return -1;
	}

	private int FindVertex(Vector3 vertex, Vector3 normal, Vector2 lmapUv)
	{
		for (int i = 0; i < m_Vertices.Count; i++)
		{
			Vector3 vector = m_Vertices[i];
			Vector3 vector2 = m_Normals[i];
			if (i >= m_LmapUv.Count || i < 0)
			{
				UnityEngine.Debug.LogError($"{m_LmapUv.Count} <-> {i} <-> {m_Vertices.Count}");
			}
			Vector2 vector3 = m_LmapUv[i];
			if (Mathf.Approximately(vector.x, vertex.x) && Mathf.Approximately(vector.y, vertex.y) && Mathf.Approximately(vector.z, vertex.z) && Mathf.Approximately(vector2.x, normal.x) && Mathf.Approximately(vector2.y, normal.y) && Mathf.Approximately(vector2.z, normal.z) && Mathf.Approximately(vector3.x, lmapUv.x) && Mathf.Approximately(vector3.y, lmapUv.y))
			{
				return i;
			}
		}
		return -1;
	}

	private void AddTexCoord(Vector3 ver, Rect uvRect)
	{
		float x = Mathf.Lerp(uvRect.xMin, uvRect.xMax, ver.x + 0.5f);
		float y = Mathf.Lerp(uvRect.yMin, uvRect.yMax, ver.y + 0.5f);
		m_TexCoords.Add(new Vector2(x, y));
	}

	public void Push(float distance)
	{
		for (int i = 0; i < m_Vertices.Count; i++)
		{
			m_Vertices[i] += m_PushNormals[i] * distance;
		}
	}

	public void ToMesh(Mesh mesh, Matrix4x4 world)
	{
		mesh.Clear(keepVertexLayout: true);
		if (m_Indices.Count != 0)
		{
			for (int i = 0; i < m_Vertices.Count; i++)
			{
				m_Vertices[i] = world.MultiplyPoint(m_Vertices[i]);
			}
			mesh.vertices = m_Vertices.ToArray();
			mesh.normals = m_Normals.ToArray();
			mesh.uv = m_TexCoords.ToArray();
			if (m_LmapUv.Count > 0)
			{
				mesh.uv2 = m_LmapUv.ToArray();
			}
			else
			{
				mesh.uv2 = m_TexCoords.ToArray();
			}
			mesh.triangles = m_Indices.ToArray();
			m_Vertices.Clear();
			m_LmapUv.Clear();
			m_Normals.Clear();
			m_PushNormals.Clear();
			m_TexCoords.Clear();
			m_Indices.Clear();
		}
	}
}
