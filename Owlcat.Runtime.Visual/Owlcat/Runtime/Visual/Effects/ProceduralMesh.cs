using System.Collections.Generic;
using System.Diagnostics;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Effects;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public abstract class ProceduralMesh : MonoBehaviour
{
	private JobHandle m_UpdateGeometryJobHandle;

	private JobHandle m_UpdateGeometryCameraJobHandle;

	protected bool m_HasCameraDependency;

	protected Mesh m_Mesh;

	protected MeshFilter m_MeshFilter;

	protected MeshRenderer m_MeshRenderer;

	protected VertexAttributeDescriptor[] m_VertexAttributeDescriptors;

	[SerializeField]
	protected Space m_Space;

	public static HashSet<ProceduralMesh> All { get; private set; }

	public VertexAttributeDescriptor[] VertexAttributeDescriptors => m_VertexAttributeDescriptors;

	public Mesh Mesh => m_Mesh;

	public MeshRenderer MeshRenderer => m_MeshRenderer;

	public Space Space
	{
		get
		{
			return m_Space;
		}
		set
		{
			m_Space = value;
		}
	}

	static ProceduralMesh()
	{
		All = new HashSet<ProceduralMesh>();
	}

	protected virtual void OnEnable()
	{
		m_Mesh = new Mesh();
		m_Mesh.name = $"ProceduralMesh_{base.gameObject}";
		m_Mesh.hideFlags = HideFlags.HideAndDontSave;
		m_MeshFilter = GetComponent<MeshFilter>();
		m_MeshFilter.sharedMesh = m_Mesh;
		m_Mesh.MarkDynamic();
		All.Add(this);
	}

	protected virtual void OnDisable()
	{
		All.Remove(this);
		Object.DestroyImmediate(m_Mesh);
	}

	protected virtual void Update()
	{
		m_HasCameraDependency = GetCameraDependency();
		JobHandle dependsOn = StartUpdateJob();
		m_UpdateGeometryJobHandle = StartUpdateGeometryJob(dependsOn);
	}

	protected virtual void LateUpdate()
	{
		m_UpdateGeometryJobHandle.Complete();
		if (!m_HasCameraDependency)
		{
			AfterUpdateJobComplete();
		}
	}

	protected abstract bool GetCameraDependency();

	protected virtual void AfterUpdateJobComplete()
	{
	}

	[Conditional("UNITY_EDITOR")]
	public virtual void SetDirtyFromEditor()
	{
	}

	protected abstract JobHandle StartUpdateJob();

	protected abstract JobHandle StartUpdateGeometryJob(JobHandle dependsOn);

	protected abstract JobHandle StartCameraUpdateGeometryJob(JobHandle dependsOn, Camera camera);

	public static void UpdateCameraDependencyAllBegin(Camera camera)
	{
		foreach (ProceduralMesh item in All)
		{
			item.m_UpdateGeometryCameraJobHandle = item.StartCameraUpdateGeometryJob(item.m_UpdateGeometryJobHandle, camera);
		}
	}

	public static void UpdateCameraDependencyAllEnd()
	{
		foreach (ProceduralMesh item in All)
		{
			item.m_UpdateGeometryCameraJobHandle.Complete();
			item.m_UpdateGeometryCameraJobHandle = default(JobHandle);
			if (item.m_HasCameraDependency)
			{
				item.AfterUpdateJobComplete();
			}
		}
	}

	public virtual string GetStats()
	{
		int subMeshCount = m_Mesh.subMeshCount;
		uint num = 0u;
		for (int i = 0; i < subMeshCount; i++)
		{
			num += m_Mesh.GetIndexCount(i);
		}
		return $"Vertex count: {m_Mesh.vertexCount}\nIndex count: {num}\nTriangle count: {num / 3}";
	}
}
