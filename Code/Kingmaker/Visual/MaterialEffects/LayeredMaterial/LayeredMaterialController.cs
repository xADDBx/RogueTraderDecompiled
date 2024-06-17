using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Visual.MaterialEffects.LayeredMaterial;

internal sealed class LayeredMaterialController
{
	private sealed class LayeredMaterialRendererCollection
	{
		public readonly RendererType rendererType;

		public readonly List<LayeredMaterialRenderer> renderers = new List<LayeredMaterialRenderer>();

		public LayeredMaterialRendererCollection(RendererType rendererType)
		{
			this.rendererType = rendererType;
		}

		public void Clear()
		{
			renderers.Clear();
		}
	}

	private readonly MaterialPropertyBlock m_MaterialPropertyBlock;

	private readonly Timeline m_Timeline;

	private readonly LayeredMaterialRendererCollection[] m_RendererCollections;

	private readonly ScriptPropertiesSnapshot m_ScriptPropertiesSnapshot;

	private int m_RenderersCount;

	public LayeredMaterialController(GameObject rootGameObject, MaterialPropertyBlock materialPropertyBlock)
	{
		m_MaterialPropertyBlock = materialPropertyBlock;
		m_Timeline = new Timeline();
		m_ScriptPropertiesSnapshot = new ScriptPropertiesSnapshot(rootGameObject);
		m_RendererCollections = new LayeredMaterialRendererCollection[2];
		for (int i = 0; i < 2; i++)
		{
			RendererType rendererType = (RendererType)(1 << i);
			m_RendererCollections[i] = new LayeredMaterialRendererCollection(rendererType);
		}
	}

	public void SetMaxActiveLayersCount(int value)
	{
		m_Timeline.SetMaxPlayingTracksCount(value);
	}

	public void RefreshScriptPropertiesSnapshot()
	{
		m_ScriptPropertiesSnapshot.CaptureStaticProperties();
	}

	public void SetupRenderers(List<Renderer> renderers)
	{
		CleanupRenderers();
		foreach (Renderer renderer3 in renderers)
		{
			if (renderer3 is SkinnedMeshRenderer renderer)
			{
				if (renderer.GetSubMeshCount() == 1)
				{
					m_RendererCollections[1].renderers.Add(LayeredMaterialRenderer.Get(renderer3));
					m_RenderersCount++;
				}
			}
			else if (renderer3 is MeshRenderer renderer2 && renderer2.GetSubMeshCount() == 1)
			{
				m_RendererCollections[0].renderers.Add(LayeredMaterialRenderer.Get(renderer3));
				m_RenderersCount++;
			}
		}
		float time = Time.time;
		m_Timeline.UpdatePlayingTracks(time);
		UpdateAdditionalMaterials();
		UpdateMaterialProperties(time);
	}

	public void CleanupRenderers()
	{
		LayeredMaterialRendererCollection[] rendererCollections = m_RendererCollections;
		foreach (LayeredMaterialRendererCollection layeredMaterialRendererCollection in rendererCollections)
		{
			foreach (LayeredMaterialRenderer renderer in layeredMaterialRendererCollection.renderers)
			{
				if (renderer.IsValid())
				{
					renderer.SetMaterials(null);
				}
				renderer.Recycle();
			}
			layeredMaterialRendererCollection.Clear();
		}
		m_RenderersCount = 0;
	}

	public void RefreshMaterialPropertiesSnapshots()
	{
		LayeredMaterialRendererCollection[] rendererCollections = m_RendererCollections;
		foreach (LayeredMaterialRendererCollection layeredMaterialRendererCollection in rendererCollections)
		{
			for (int num = layeredMaterialRendererCollection.renderers.Count - 1; num >= 0; num--)
			{
				LayeredMaterialRenderer layeredMaterialRenderer = layeredMaterialRendererCollection.renderers[num];
				if (layeredMaterialRenderer.IsValid())
				{
					layeredMaterialRenderer.RefreshBaseMaterialPropertiesSnapshot();
				}
				else
				{
					layeredMaterialRendererCollection.renderers.RemoveAt(num);
				}
			}
		}
	}

	public bool TryAddAnimation(LayeredMaterialAnimationSetup animationSetup, out int token)
	{
		return animationSetup.TryAddTrack(m_Timeline, out token);
	}

	public void RemoveAnimation(int token)
	{
		m_Timeline.RemoveTrack(token);
	}

	public void Update()
	{
		RemoveInvalidRendererData();
		float time = Time.time;
		if (m_Timeline.UpdatePlayingTracks(time))
		{
			UpdateAdditionalMaterials();
		}
		UpdateMaterialProperties(time);
	}

	private void UpdateAdditionalMaterials()
	{
		LayeredMaterialRendererCollection[] rendererCollections = m_RendererCollections;
		foreach (LayeredMaterialRendererCollection layeredMaterialRendererCollection in rendererCollections)
		{
			List<Material> value;
			using (CollectionPool<List<Material>, Material>.Get(out value))
			{
				m_Timeline.GetPlayingTracksMaterials(layeredMaterialRendererCollection.rendererType, value);
				foreach (LayeredMaterialRenderer renderer in layeredMaterialRendererCollection.renderers)
				{
					renderer.SetMaterials(value);
				}
			}
		}
	}

	private void UpdateMaterialProperties(float time)
	{
		if (m_RenderersCount == 0)
		{
			return;
		}
		m_ScriptPropertiesSnapshot.CaptureDynamicProperties();
		LayeredMaterialRendererCollection[] rendererCollections = m_RendererCollections;
		foreach (LayeredMaterialRendererCollection layeredMaterialRendererCollection in rendererCollections)
		{
			List<Track> value;
			using (CollectionPool<List<Track>, Track>.Get(out value))
			{
				m_Timeline.GetPlayingTracks(layeredMaterialRendererCollection.rendererType, value);
				foreach (LayeredMaterialRenderer renderer in layeredMaterialRendererCollection.renderers)
				{
					PropertyBlock properties = new PropertyBlock(m_ScriptPropertiesSnapshot, renderer.GetBaseMaterialPropertiesSnapshot(), m_MaterialPropertyBlock);
					int j = 0;
					for (int count = value.Count; j < count; j++)
					{
						value[j].Sample(in properties, time);
						renderer.SetMaterialPropertyBlock(m_MaterialPropertyBlock, j);
						m_MaterialPropertyBlock.Clear();
					}
				}
			}
		}
	}

	private void RemoveInvalidRendererData()
	{
		LayeredMaterialRendererCollection[] rendererCollections = m_RendererCollections;
		for (int i = 0; i < rendererCollections.Length; i++)
		{
			List<LayeredMaterialRenderer> renderers = rendererCollections[i].renderers;
			for (int num = renderers.Count - 1; num >= 0; num--)
			{
				if (!renderers[num].IsValid())
				{
					renderers.RemoveAt(num);
				}
			}
		}
	}
}
