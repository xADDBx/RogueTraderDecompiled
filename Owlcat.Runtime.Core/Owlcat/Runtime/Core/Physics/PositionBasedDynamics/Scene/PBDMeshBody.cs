using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;

public class PBDMeshBody : PBDBodyBase<MeshBody>
{
	private static int _PbdParticlesOffset = Shader.PropertyToID("_PbdParticlesOffset");

	private static int _PbdEnabledLocal = Shader.PropertyToID("_PbdEnabledLocal");

	private static int _PbdVertexOffset = Shader.PropertyToID("_PbdVertexOffset");

	private static int _PbdBodyDescriptorIndex = Shader.PropertyToID("_PbdBodyDescriptorIndex");

	[SerializeField]
	private bool m_UseGlobalGravity = true;

	[SerializeField]
	private bool m_UseGlobalWind = true;

	[SerializeField]
	public bool m_RecalculateNormalsAndTangentsOnEveryFrame = true;

	[SerializeField]
	private Bounds m_SimulatedBounds;

	[SerializeField]
	private int[] m_ParentList;

	[SerializeField]
	private List<int> m_VertexTriangleMap = new List<int>();

	[SerializeField]
	private List<int> m_VertexTriangleMapOffsetCount = new List<int>();

	[SerializeField]
	private List<Vector3> m_Vertices = new List<Vector3>();

	[SerializeField]
	private List<Vector3> m_Normals = new List<Vector3>();

	[SerializeField]
	private List<Vector2> m_Uv = new List<Vector2>();

	[SerializeField]
	private List<Vector4> m_Tangents = new List<Vector4>();

	private MeshFilter m_MeshFilter;

	private MeshRenderer m_MeshRenderer;

	private Mesh m_Mesh;

	private MaterialPropertyBlock m_MaterialPropertyBlock;

	public Bounds SimulatedBounds
	{
		get
		{
			return m_SimulatedBounds;
		}
		set
		{
			m_SimulatedBounds = value;
		}
	}

	public int[] ParentList
	{
		get
		{
			return m_ParentList;
		}
		set
		{
			m_ParentList = value;
		}
	}

	public List<int> VertexTriangleMap => m_VertexTriangleMap;

	public List<int> VertexTriangleMapOffsetCount => m_VertexTriangleMapOffsetCount;

	public List<Vector3> Vertices => m_Vertices;

	public List<Vector3> Normals => m_Normals;

	public List<Vector2> Uv => m_Uv;

	public List<Vector4> Tangents => m_Tangents;

	protected override bool ValidateBeforeInitialize()
	{
		return true;
	}

	protected override void InitializeInternal()
	{
		InitBody();
		if (m_Body != null)
		{
			PBD.RegisterBody(m_Body);
			RegisterLocalColliders();
			if (PBD.IsGpu)
			{
				m_MaterialPropertyBlock = new MaterialPropertyBlock();
				m_MaterialPropertyBlock.SetFloat(_PbdEnabledLocal, 0f);
				m_MeshRenderer.SetPropertyBlock(m_MaterialPropertyBlock);
			}
			else
			{
				PBD.OnAfterUpdate = (Action)Delegate.Combine(PBD.OnAfterUpdate, new Action(OnAfterUpdate));
			}
		}
	}

	protected override void Dispose()
	{
		UnregisterLocalColliders();
		PBD.UnregisterBody(m_Body);
		m_Body.Dispose();
		m_Body = null;
		if (!PBD.IsGpu)
		{
			PBD.OnAfterUpdate = (Action)Delegate.Remove(PBD.OnAfterUpdate, new Action(OnAfterUpdate));
		}
	}

	protected internal override void OnBodyDataUpdated()
	{
		if (PBD.IsGpu)
		{
			m_MaterialPropertyBlock.SetFloat(_PbdEnabledLocal, 1f);
			m_MaterialPropertyBlock.SetFloat(_PbdParticlesOffset, base.Body.ParticlesOffset);
			m_MaterialPropertyBlock.SetFloat(_PbdVertexOffset, base.Body.VertexOffset);
			m_MaterialPropertyBlock.SetFloat(_PbdBodyDescriptorIndex, PBD.GetBodyDescriptorIndex(GetBody()));
			m_MeshRenderer.SetPropertyBlock(m_MaterialPropertyBlock);
		}
	}

	private void InitBody()
	{
		if (m_Body != null)
		{
			return;
		}
		m_MeshFilter = GetComponent<MeshFilter>();
		m_MeshRenderer = GetComponent<MeshRenderer>();
		if (m_MeshFilter == null)
		{
			return;
		}
		if (PBD.IsGpu)
		{
			m_Mesh = m_MeshFilter.sharedMesh;
		}
		else
		{
			m_Mesh = m_MeshFilter.mesh;
		}
		Bounds bounds = m_Mesh.bounds;
		bounds.Encapsulate(m_SimulatedBounds);
		m_Mesh.bounds = bounds;
		List<int> list = new List<int>();
		List<int> list2 = ListPool<int>.Claim();
		for (int i = 0; i < m_Mesh.subMeshCount; i++)
		{
			list2.Clear();
			m_Mesh.GetTriangles(list2, i, applyBaseVertex: true);
			list.AddRange(list2);
		}
		m_MeshFilter.mesh = m_Mesh;
		List<Particle> list3 = new List<Particle>();
		List<Constraint> list4 = new List<Constraint>();
		for (int j = 0; j < m_Particles.Count; j++)
		{
			Particle item = m_Particles[j];
			item.Position = base.transform.TransformPoint(item.Position);
			item.BasePosition = item.Position;
			item.Orientation = base.transform.rotation * item.Orientation;
			item.PredictedOrientation = item.Orientation;
			if (!m_UseGlobalGravity)
			{
				item.Flags |= 1u;
			}
			if (!m_UseGlobalWind)
			{
				item.Flags |= 2u;
			}
			list3.Add(item);
		}
		for (int k = 0; k < m_Constraints.Count; k++)
		{
			Constraint item2 = m_Constraints[k];
			item2.Refresh(list3);
			list4.Add(item2);
		}
		m_Body = new MeshBody(base.name, list3, list4, base.DisconnectedConstraintsOffsetCount, m_Vertices, m_Normals, m_Tangents, m_Uv, list, m_VertexTriangleMap, m_VertexTriangleMapOffsetCount);
		m_Body.Restitution = base.Restitution;
		m_Body.Friction = base.Friction;
		m_Body.TeleportDistanceTreshold = base.TeleportDistanceTreshold;
		m_Body.LocalToWorld = base.transform.localToWorldMatrix;
		ListPool<int>.Release(list2);
	}

	private void LateUpdate()
	{
	}

	private void OnAfterUpdate()
	{
		if (!PBD.IsGpu && m_RecalculateNormalsAndTangentsOnEveryFrame)
		{
			MeshBodyVerticesSoA meshData = PBD.GetMeshData();
			m_Mesh.SetVertices(meshData.Vertices, base.Body.VertexOffset, base.Body.Vertices.Count);
			m_Mesh.SetNormals(meshData.Normals, base.Body.VertexOffset, base.Body.Vertices.Count);
			m_Mesh.SetTangents(meshData.Tangents, base.Body.VertexOffset, base.Body.Vertices.Count);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!PBD.IsGpu)
		{
			PBD.DrawGizmos(m_Body);
		}
	}

	public MeshRenderer GetRenderer()
	{
		return m_MeshRenderer;
	}
}
