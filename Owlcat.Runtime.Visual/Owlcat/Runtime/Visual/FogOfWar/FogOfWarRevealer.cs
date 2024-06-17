using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace Owlcat.Runtime.Visual.FogOfWar;

public class FogOfWarRevealer
{
	private static HashSet<FogOfWarRevealer> s_All = new HashSet<FogOfWarRevealer>();

	private static Mesh s_EmptyMesh;

	private Mesh m_ShadowMesh;

	private List<FogOfWarBlocker> m_Blockers = new List<FogOfWarBlocker>();

	private List<FogOfWarBlocker> m_Blockers2 = new List<FogOfWarBlocker>();

	private List<(Vector3 pos, Quaternion rot)> m_BlockersTransforms = new List<(Vector3, Quaternion)>();

	private CombineInstance[] m_CombineInstances = new CombineInstance[0];

	public float Range = 10f;

	public float Radius = 1f;

	public Vector2 HeightMinMax;

	public Vector3 Position;

	public float Rotation;

	public Vector2 Scale;

	public Texture2D MaskTexture;

	private NativeArray<Vector3> m_CombinedVertices;

	private NativeArray<Vector3> m_CombinedNormals;

	private NativeArray<int> m_CombinedIndices;

	public Mesh ShadowMesh => m_ShadowMesh;

	public static Vector4[] DefaultVertices { get; } = new Vector4[4]
	{
		new Vector4(0f, 0f, 0f, 1f),
		new Vector4(1f, 0f, 0f, 1f),
		new Vector4(0f, 1f, 0f, 1f),
		new Vector4(1f, 1f, 0f, 1f)
	};


	public static HashSet<FogOfWarRevealer> All => s_All;

	public static void Register(FogOfWarRevealer revealer)
	{
		s_All.Add(revealer);
	}

	public static void Unregister(FogOfWarRevealer revealer)
	{
		s_All.Remove(revealer);
	}

	public void RebuildShadowMesh()
	{
		bool flag = false;
		m_Blockers2.Clear();
		foreach (FogOfWarBlocker value in FogOfWarBlocker.All.Values)
		{
			if (!value.Indices.IsCreated || (HeightMinMax.x > value.HeightMinMax.y && HeightMinMax.y > value.HeightMinMax.y) || (HeightMinMax.x < value.HeightMinMax.x && HeightMinMax.y < value.HeightMinMax.x))
			{
				continue;
			}
			float num = Position.x - value.Center.x;
			float num2 = Position.z - value.Center.y;
			float num3 = num * num + num2 * num2;
			float num4 = Range + value.Radius.x;
			num4 *= num4;
			if (!(num3 - num4 <= 0f))
			{
				continue;
			}
			m_Blockers2.Add(value);
			if (!flag && m_Blockers2.Count <= m_Blockers.Count)
			{
				int index = m_Blockers2.Count - 1;
				FogOfWarBlocker fogOfWarBlocker = m_Blockers2[index];
				if (fogOfWarBlocker != m_Blockers[index])
				{
					flag = true;
				}
				else if (fogOfWarBlocker.transform.position != m_BlockersTransforms[index].pos || fogOfWarBlocker.transform.rotation != m_BlockersTransforms[index].rot)
				{
					flag = true;
				}
			}
		}
		if (m_Blockers2.Count != m_Blockers.Count)
		{
			flag = true;
		}
		if (flag || m_ShadowMesh == null)
		{
			m_Blockers.Clear();
			m_Blockers.InsertRange(0, m_Blockers2);
			m_BlockersTransforms.Clear();
			m_BlockersTransforms.InsertRange(0, m_Blockers.Select((FogOfWarBlocker b) => (position: b.transform.position, rotation: b.transform.rotation)));
			RebuildShadowMesh(m_Blockers);
		}
	}

	public void RebuildShadowMesh(List<FogOfWarBlocker> polygons)
	{
		if (m_ShadowMesh == null)
		{
			InitMesh();
		}
		m_ShadowMesh.Clear();
		if (polygons.Count == 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		foreach (FogOfWarBlocker polygon in polygons)
		{
			num += polygon.SegmentA_Index.Length;
			num2 += polygon.Indices.Length;
		}
		if (!m_CombinedVertices.IsCreated || m_CombinedVertices.Length < num)
		{
			if (m_CombinedVertices.IsCreated)
			{
				m_CombinedVertices.Dispose();
			}
			if (m_CombinedNormals.IsCreated)
			{
				m_CombinedNormals.Dispose();
			}
			if (m_CombinedIndices.IsCreated)
			{
				m_CombinedIndices.Dispose();
			}
			m_CombinedVertices = new NativeArray<Vector3>(num, Allocator.Persistent);
			m_CombinedNormals = new NativeArray<Vector3>(num, Allocator.Persistent);
			m_CombinedIndices = new NativeArray<int>(num2, Allocator.Persistent);
		}
		int num3 = 0;
		int num4 = 0;
		foreach (FogOfWarBlocker polygon2 in polygons)
		{
			NativeArray<Vector3>.Copy(polygon2.SegmentA_Index, 0, m_CombinedVertices, num4, polygon2.SegmentA_Index.Length);
			NativeArray<Vector3>.Copy(polygon2.SegmentB_Offset, 0, m_CombinedNormals, num4, polygon2.SegmentB_Offset.Length);
			NativeArray<int>.Copy(polygon2.Indices, 0, m_CombinedIndices, num3, polygon2.Indices.Length);
			for (int i = 0; i < polygon2.Indices.Length; i++)
			{
				m_CombinedIndices[i + num3] += num4;
			}
			num4 += polygon2.SegmentA_Index.Length;
			num3 += polygon2.Indices.Length;
		}
		m_ShadowMesh.SetVertices(m_CombinedVertices, 0, num);
		m_ShadowMesh.SetNormals(m_CombinedNormals, 0, num);
		m_ShadowMesh.SetIndices(m_CombinedIndices, 0, num2, MeshTopology.Triangles, 0);
	}

	private void InitMesh()
	{
		m_ShadowMesh = new Mesh
		{
			name = "FogOfWarRevealerMesh"
		};
		m_ShadowMesh.MarkDynamic();
	}

	private static Vector2 Transform2D(Matrix4x4 m, Vector2 p)
	{
		return new Vector2(p.x * m[0] + p.y * m[4] + m[12], p.x * m[1] + p.y * m[5] + m[13]);
	}

	public void OnDisabled()
	{
		if (m_CombinedVertices.IsCreated)
		{
			m_CombinedVertices.Dispose();
		}
		if (m_CombinedNormals.IsCreated)
		{
			m_CombinedNormals.Dispose();
		}
		if (m_CombinedIndices.IsCreated)
		{
			m_CombinedIndices.Dispose();
		}
	}

	public void Dispose()
	{
		OnDisabled();
		if (m_ShadowMesh != null)
		{
			Object.DestroyImmediate(m_ShadowMesh);
		}
	}
}
