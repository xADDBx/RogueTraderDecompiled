using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Core.Math;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace Owlcat.Runtime.Visual.FogOfWar;

public class FogOfWarBlocker : MonoBehaviour
{
	private class FogOfWarTransformPositionComparer : IComparer<FogOfWarBlocker>
	{
		public int Compare(FogOfWarBlocker x, FogOfWarBlocker y)
		{
			if (x == y)
			{
				return 0;
			}
			if (y == null)
			{
				return 1;
			}
			if (x == null)
			{
				return -1;
			}
			Vector3 position = x.transform.position;
			Vector3 position2 = y.transform.position;
			if (!Mathf.Approximately(position.x, position2.x))
			{
				if (!(position.x < position2.x))
				{
					return -1;
				}
				return 1;
			}
			if (!Mathf.Approximately(position.y, position2.y))
			{
				if (!(position.y < position2.y))
				{
					return -1;
				}
				return 1;
			}
			if (!Mathf.Approximately(position.z, position2.z))
			{
				if (!(position.z < position2.z))
				{
					return -1;
				}
				return 1;
			}
			return 0;
		}
	}

	public delegate void ActivateEventHandler(FogOfWarBlocker blocker);

	public delegate void DeactivateEventHandler(FogOfWarBlocker blocker);

	public delegate void RebuildEventHandler(FogOfWarBlocker blocker);

	public delegate void ChangeEventHandler(FogOfWarBlocker blocker);

	public static readonly SortedList<FogOfWarBlocker, FogOfWarBlocker> All = new SortedList<FogOfWarBlocker, FogOfWarBlocker>(new FogOfWarTransformPositionComparer());

	private bool m_Active;

	private Transform m_CachedTransform;

	private Matrix4x4 m_CachedTransformMatrix;

	private float m_CachedShadowOffset;

	private float m_CachedGlobalShadowOffset;

	private Vector2[] m_Points;

	private bool m_PointsClosed;

	private NativeArray<Vector3> m_MeshSegmentAIndex;

	private NativeArray<Vector3> m_MeshSegmentBOffset;

	private NativeArray<int> m_MeshIndices;

	public float ShadowFalloff;

	public Vector2 HeightMinMax;

	public Vector2 Center;

	public Vector2 Radius;

	public bool TwoSided;

	public Vector2[] Points => m_Points;

	public bool Closed => m_PointsClosed;

	public NativeArray<Vector3> SegmentA_Index => m_MeshSegmentAIndex;

	public NativeArray<Vector3> SegmentB_Offset => m_MeshSegmentBOffset;

	public NativeArray<int> Indices => m_MeshIndices;

	public static event ActivateEventHandler BlockerActivated;

	public static event DeactivateEventHandler BlockerDeactivated;

	public static event ChangeEventHandler BlockerChanged;

	private void OnEnable()
	{
		MeshFilter component = GetComponent<MeshFilter>();
		FogOfWarBlockerPolygon component2 = GetComponent<FogOfWarBlockerPolygon>();
		if (component == null && component2 == null)
		{
			Debug.LogError("FogOfWarBlocker: " + base.name + " can't build mesh because there is no MeshFilter of FogOfWarBlockerPolygon", this);
			base.enabled = false;
			return;
		}
		FogOfWarArea active = FogOfWarArea.Active;
		m_CachedTransform = base.transform;
		m_CachedTransformMatrix = m_CachedTransform.localToWorldMatrix;
		m_CachedShadowOffset = ShadowFalloff;
		m_CachedGlobalShadowOffset = ((active != null) ? active.ShadowFalloff : 0f);
		BuildPoints();
		BuildMesh();
		m_Active = true;
		if (!All.ContainsKey(this))
		{
			All.Add(this, this);
		}
		NotifyActivate();
	}

	private void OnDisable()
	{
		if (m_MeshSegmentAIndex.IsCreated)
		{
			m_MeshSegmentAIndex.Dispose();
			m_MeshSegmentAIndex = default(NativeArray<Vector3>);
		}
		if (m_MeshSegmentBOffset.IsCreated)
		{
			m_MeshSegmentBOffset.Dispose();
			m_MeshSegmentBOffset = default(NativeArray<Vector3>);
		}
		if (Indices.IsCreated)
		{
			m_MeshIndices = default(NativeArray<int>);
			m_MeshIndices.Dispose();
		}
		if (m_Active)
		{
			m_Active = false;
			All.Remove(this);
			NotifyDeactivate();
		}
	}

	public void UpdateIfNecessary()
	{
		if (!m_Active)
		{
			return;
		}
		try
		{
			FogOfWarArea active = FogOfWarArea.Active;
			float num = ((active != null) ? active.ShadowFalloff : 0f);
			Matrix4x4 matrix4x;
			bool flag;
			if (m_CachedTransform.hasChanged)
			{
				matrix4x = m_CachedTransform.localToWorldMatrix;
				flag = matrix4x != m_CachedTransformMatrix;
			}
			else
			{
				matrix4x = default(Matrix4x4);
				flag = false;
			}
			if (flag || m_CachedGlobalShadowOffset != num || m_CachedShadowOffset != ShadowFalloff)
			{
				try
				{
					BuildPoints();
					BuildMesh();
					NotifyChanged();
				}
				finally
				{
				}
				m_CachedTransform.hasChanged = false;
				m_CachedTransformMatrix = matrix4x;
				m_CachedShadowOffset = ShadowFalloff;
				m_CachedGlobalShadowOffset = num;
			}
		}
		finally
		{
		}
	}

	public void BuildPoints()
	{
		MeshFilter component2;
		if (TryGetComponent<FogOfWarBlockerPolygon>(out var component))
		{
			BuildPointsFromPolygon(component);
		}
		else if (TryGetComponent<MeshFilter>(out component2))
		{
			Mesh sharedMesh = component2.sharedMesh;
			if (sharedMesh != null)
			{
				BuildPointsFromMesh(base.transform, sharedMesh);
			}
		}
		else
		{
			m_Points = Array.Empty<Vector2>();
			m_PointsClosed = false;
		}
	}

	private void BuildPointsFromPolygon(FogOfWarBlockerPolygon poly)
	{
		Vector3[] transformedPoints = poly.TransformedPoints;
		Vector3 lhs = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 lhs2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		foreach (Vector3 rhs in transformedPoints)
		{
			lhs = Vector3.Min(lhs, rhs);
			lhs2 = Vector3.Max(lhs2, rhs);
		}
		float num = lhs2.x - lhs.x;
		float num2 = lhs2.z - lhs.z;
		Center = new Vector2(num / 2f + lhs.x, num2 / 2f + lhs.z);
		float num3 = Vector2.Distance(new Vector2(lhs.x, lhs.z), new Vector2(lhs2.x, lhs2.z)) / 2f;
		Radius = new Vector2(num3, num3 * num3);
		HeightMinMax = new Vector2(lhs.y - poly.Height, lhs2.y + poly.Height);
		int num4 = (TwoSided ? (transformedPoints.Length * 2 - 1) : transformedPoints.Length);
		if (m_Points == null || m_Points.Length != num4)
		{
			m_Points = new Vector2[num4];
		}
		for (int j = 0; j < transformedPoints.Length; j++)
		{
			Vector2 vector = new Vector2(transformedPoints[j].x, transformedPoints[j].z);
			m_Points[j] = vector;
			if (TwoSided && j > 0)
			{
				m_Points[m_Points.Length - j] = vector;
			}
		}
		m_PointsClosed = poly.Closed || TwoSided;
	}

	private void BuildPointsFromMesh(Transform root, Mesh mesh)
	{
		Vector3[] vertices = mesh.vertices;
		Vector3[] array = new Vector3[vertices.Length];
		Vector3 lhs = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 lhs2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 vector = root.TransformPoint(vertices[i]);
			lhs = Vector3.Min(lhs, vector);
			lhs2 = Vector3.Max(lhs2, vector);
			array[i] = vector;
		}
		float num = lhs2.x - lhs.x;
		float num2 = lhs2.z - lhs.z;
		Center = new Vector2(num / 2f + lhs.x, num2 / 2f + lhs.z);
		float num3 = Vector2.Distance(new Vector2(lhs.x, lhs.z), new Vector2(lhs2.x, lhs2.z)) / 2f;
		Radius = new Vector2(num3, num3 * num3);
		HeightMinMax = new Vector2(lhs.y, lhs2.y);
		List<Vector3> value;
		using (CollectionPool<List<Vector3>, Vector3>.Get(out value))
		{
			Polygon.ConvexHullXZ(array, value);
			if (m_Points == null || m_Points.Length != value.Count)
			{
				m_Points = new Vector2[value.Count];
			}
			for (int j = 0; j < value.Count; j++)
			{
				m_Points[j] = new Vector2(value[j].x, value[j].z);
			}
		}
		m_PointsClosed = true;
	}

	private void BuildMesh()
	{
		EnsureNativeArraySize(ref m_MeshSegmentAIndex, m_Points.Length * 4);
		EnsureNativeArraySize(ref m_MeshSegmentBOffset, m_Points.Length * 4);
		EnsureNativeArraySize(ref m_MeshIndices, m_Points.Length * 6);
		FogOfWarArea active = FogOfWarArea.Active;
		float z = ((ShadowFalloff > 0f) ? ShadowFalloff : ((active == null) ? 0f : active.ShadowFalloff));
		if (m_Points.Length >= 2)
		{
			int num = 0;
			Vector2 vector = m_Points[m_PointsClosed ? (m_Points.Length - 1) : 0];
			for (int i = ((!m_PointsClosed) ? 1 : 0); i < m_Points.Length; i++)
			{
				int num2 = i;
				Vector2 vector2 = m_Points[num2];
				m_MeshSegmentAIndex[num * 4] = new Vector3(vector.x, vector.y, 0f);
				m_MeshSegmentAIndex[num * 4 + 1] = new Vector3(vector.x, vector.y, 1f);
				m_MeshSegmentAIndex[num * 4 + 2] = new Vector3(vector.x, vector.y, 2f);
				m_MeshSegmentAIndex[num * 4 + 3] = new Vector3(vector.x, vector.y, 3f);
				m_MeshSegmentBOffset[num * 4] = new Vector3(vector2.x, vector2.y, z);
				m_MeshSegmentBOffset[num * 4 + 1] = new Vector3(vector2.x, vector2.y, z);
				m_MeshSegmentBOffset[num * 4 + 2] = new Vector3(vector2.x, vector2.y, z);
				m_MeshSegmentBOffset[num * 4 + 3] = new Vector3(vector2.x, vector2.y, z);
				m_MeshIndices[num * 6] = num * 4;
				m_MeshIndices[num * 6 + 1] = num * 4 + 1;
				m_MeshIndices[num * 6 + 2] = num * 4 + 2;
				m_MeshIndices[num * 6 + 3] = num * 4 + 1;
				m_MeshIndices[num * 6 + 4] = num * 4 + 3;
				m_MeshIndices[num * 6 + 5] = num * 4 + 2;
				num++;
				vector = vector2;
			}
		}
	}

	private void NotifyActivate()
	{
		FogOfWarBlocker.BlockerActivated?.Invoke(this);
	}

	private void NotifyDeactivate()
	{
		FogOfWarBlocker.BlockerDeactivated?.Invoke(this);
	}

	private void NotifyChanged()
	{
		FogOfWarBlocker.BlockerChanged?.Invoke(this);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void EnsureNativeArraySize<T>(ref NativeArray<T> array, int size) where T : struct
	{
		if (size == array.Length)
		{
			return;
		}
		if (size > 0)
		{
			if (array.IsCreated)
			{
				array.Dispose();
			}
			array = new NativeArray<T>(size, Allocator.Persistent);
		}
		else if (array.IsCreated)
		{
			array.Dispose();
			array = default(NativeArray<T>);
		}
	}
}
