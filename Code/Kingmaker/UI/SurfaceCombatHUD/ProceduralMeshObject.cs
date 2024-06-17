using System;
using System.Collections.Generic;
using Kingmaker.Visual.MaterialEffects;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.UI.SurfaceCombatHUD;

internal class ProceduralMeshObject<TVertex, TIndex> : IDisposable where TVertex : unmanaged where TIndex : unmanaged
{
	private static readonly int kIconTextureId = Shader.PropertyToID("_IconTexture");

	private static readonly int kHighlightStartTime = Shader.PropertyToID("_HighlightStartTime");

	private static readonly int kHighlightDuration = Shader.PropertyToID("_HighlightDuration");

	private const MeshUpdateFlags kDontUpdateMeshFlags = MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds;

	private readonly MeshRenderer m_MeshRenderer;

	private readonly MeshFilter m_MeshFilter;

	private readonly VertexAttributeDescriptor[] m_VertexAttributeDescriptors;

	private readonly IndexFormat m_IndexFormat;

	private readonly MaterialPropertyBlock m_PropertyBlock;

	private Mesh m_Mesh;

	public ProceduralMeshObject(MeshRenderer meshRenderer, MeshFilter meshFilter, VertexAttributeDescriptor[] vertexAttributeDescriptors, IndexFormat indexFormat)
	{
		m_MeshRenderer = meshRenderer;
		m_MeshFilter = meshFilter;
		m_VertexAttributeDescriptors = vertexAttributeDescriptors;
		m_IndexFormat = indexFormat;
		m_Mesh = new Mesh();
		m_PropertyBlock = new MaterialPropertyBlock();
	}

	public void Clear()
	{
		if (m_MeshRenderer != null)
		{
			m_MeshRenderer.enabled = false;
		}
	}

	public void Update(ProceduralMesh<TVertex, TIndex> proceduralMesh, List<MaterialData> materials, bool applyMaterialPropertyOverrides = true)
	{
		if (proceduralMesh.IsEmpty())
		{
			if (m_MeshRenderer != null)
			{
				m_MeshRenderer.enabled = false;
			}
			return;
		}
		m_Mesh.SetVertexBufferParams(proceduralMesh.vertexBuffer.Length, m_VertexAttributeDescriptors);
		m_Mesh.SetIndexBufferParams(proceduralMesh.indexBuffer.Length, m_IndexFormat);
		m_Mesh.SetVertexBufferData(proceduralMesh.vertexBuffer.AsArray(), 0, 0, proceduralMesh.vertexBuffer.Length, 0, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds);
		m_Mesh.SetIndexBufferData<TIndex>(proceduralMesh.indexBuffer, 0, 0, proceduralMesh.indexBuffer.Length, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds);
		NativeArray<SubMeshDescriptor> desc = new NativeArray<SubMeshDescriptor>(proceduralMesh.subMeshes.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		try
		{
			int i = 0;
			for (int length = proceduralMesh.subMeshes.Length; i < length; i++)
			{
				ProcesuralSubMesh procesuralSubMesh = proceduralMesh.subMeshes[i];
				desc[i] = new SubMeshDescriptor(procesuralSubMesh.indexStart, procesuralSubMesh.indexCount);
			}
			m_Mesh.SetSubMeshes(desc);
		}
		finally
		{
			desc.Dispose();
		}
		if (m_MeshFilter != null)
		{
			m_MeshFilter.sharedMesh = m_Mesh;
		}
		if (!(m_MeshRenderer != null))
		{
			return;
		}
		List<Material> value;
		using (ListPool<Material>.Get(out value))
		{
			foreach (ProcesuralSubMesh subMesh in proceduralMesh.subMeshes)
			{
				value.Add(materials[subMesh.materialId].material);
			}
			m_MeshRenderer.SetSharedMaterialsExt(value);
		}
		if (applyMaterialPropertyOverrides)
		{
			int num = 0;
			foreach (ProcesuralSubMesh subMesh2 in proceduralMesh.subMeshes)
			{
				if (materials[subMesh2.materialId].overrides.iconOverride != null)
				{
					m_PropertyBlock.SetTexture(kIconTextureId, materials[subMesh2.materialId].overrides.iconOverride);
				}
				m_PropertyBlock.SetFloat(kHighlightStartTime, materials[subMesh2.materialId].overrides.highlightOverride.startTime);
				m_PropertyBlock.SetFloat(kHighlightDuration, materials[subMesh2.materialId].overrides.highlightOverride.diration);
				m_MeshRenderer.SetPropertyBlock(m_PropertyBlock, num++);
				m_PropertyBlock.Clear();
			}
		}
		m_MeshRenderer.enabled = true;
	}

	public void Dispose()
	{
		if (m_Mesh != null)
		{
			UnityEngine.Object.Destroy(m_Mesh);
			m_Mesh = null;
		}
	}
}
