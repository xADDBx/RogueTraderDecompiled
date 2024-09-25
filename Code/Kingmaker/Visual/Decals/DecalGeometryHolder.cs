using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Lightmapping;
using UnityEngine;

namespace Kingmaker.Visual.Decals;

[ExecuteInEditMode]
public class DecalGeometryHolder : MonoBehaviour
{
	[Serializable]
	public class RendererLmapIndex
	{
		public MeshRenderer DecalRenderer;

		public string SubstrateRendererDataId;

		public int SubstrateRendererIndex;

		public RendererLmapIndex(MeshRenderer decal, MeshRenderer substrate)
		{
			LightmapStorage current = LightmapStorage.Current;
			if ((bool)current)
			{
				SubstrateRendererDataId = current.GetRendererData(substrate, out SubstrateRendererIndex)?.Id;
			}
			else
			{
				LightmapRenderersData lightmapRenderersData = (from g in substrate.gameObject.scene.GetRootGameObjects()
					select g.GetComponent<LightmapRenderersData>()).Valid().FirstOrDefault();
				if ((bool)lightmapRenderersData)
				{
					SubstrateRendererDataId = lightmapRenderersData.Id;
					SubstrateRendererIndex = Array.IndexOf(lightmapRenderersData.Renderers, substrate);
				}
			}
			DecalRenderer = decal;
		}
	}

	[InspectorReadOnly]
	public string UniqueId;

	[InspectorReadOnly]
	public Decal Decal;

	[InspectorReadOnly]
	public Texture Texture;

	[InspectorReadOnly]
	public Texture2D ParametersTex;

	[InspectorReadOnly]
	public Material Material;

	[InspectorReadOnly]
	public float UsedTextureMemory;

	[SerializeField]
	private List<RendererLmapIndex> m_LmapIndices = new List<RendererLmapIndex>();

	private void Awake()
	{
		ApplyLightmapIndices();
		LightmapStorage.LightmapSettingsChanged += UpdateLightmapIndices;
	}

	private void OnDestroy()
	{
		LightmapStorage.LightmapSettingsChanged -= UpdateLightmapIndices;
	}

	private void UpdateLightmapIndices()
	{
		ApplyLightmapIndices();
	}

	private void ApplyLightmapIndices()
	{
		LightmapStorage current = LightmapStorage.Current;
		if (!current)
		{
			return;
		}
		foreach (RendererLmapIndex lmapIndex in m_LmapIndices)
		{
			if ((bool)lmapIndex.DecalRenderer)
			{
				Renderer rendererByData = current.GetRendererByData(lmapIndex.SubstrateRendererDataId, lmapIndex.SubstrateRendererIndex);
				if ((bool)rendererByData)
				{
					lmapIndex.DecalRenderer.lightmapIndex = rendererByData.lightmapIndex;
					lmapIndex.DecalRenderer.lightmapScaleOffset = rendererByData.lightmapScaleOffset;
				}
			}
		}
	}

	public void AddLightmapIndex(MeshRenderer decalRenderer, MeshRenderer substrateRenderer)
	{
		m_LmapIndices.Add(new RendererLmapIndex(decalRenderer, substrateRenderer));
	}

	public void ClearLightmapIndices()
	{
		m_LmapIndices.Clear();
	}
}
