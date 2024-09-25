using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Visual.Lightmapping;

[ExecuteInEditMode]
public class LightmapStorage : MonoBehaviour
{
	[Serializable]
	private class RendererData
	{
		[UsedImplicitly]
		public int LightmapIndex;

		[UsedImplicitly]
		public Vector4 ScaleOffset;
	}

	[Serializable]
	private class SphericalHarmonics
	{
		[SerializeField]
		public float[] Coefficients = new float[27];
	}

	[Serializable]
	private class LightmapData
	{
		[UsedImplicitly]
		public Texture2D LightmapColor;

		[UsedImplicitly]
		public Texture2D LightmapDir;

		[UsedImplicitly]
		public Texture2D ShadowMask;
	}

	[Serializable]
	private class Chunk
	{
		[SerializeField]
		public string RelatedRenderersDataId;

		[SerializeField]
		public RendererData[] Renderers;
	}

	public static bool ShadowmaskDisabled;

	public static readonly List<LightmapStorage> Instances = new List<LightmapStorage>();

	[SerializeField]
	[HideInInspector]
	private Chunk[] m_Chunks = new Chunk[0];

	[SerializeField]
	private LightmapData[] m_Lightmaps = new LightmapData[0];

	[SerializeField]
	[HideInInspector]
	private SphericalHarmonics[] m_BakedLightProbes;

	[SerializeField]
	[HideInInspector]
	private LightProbes m_LightProbes;

	private LightmapRenderersData[] m_RendererData;

	public static LightmapStorage Current { get; private set; }

	public static event Action LightmapSettingsChanged;

	public void ApplyLightmapSettings()
	{
		Current = this;
		if (m_Lightmaps == null)
		{
			return;
		}
		m_RendererData = LightmapRenderersData.Instances.ToArray();
		Chunk[] chunks = m_Chunks;
		foreach (Chunk chunk in chunks)
		{
			ApplyLightmapSettings(chunk);
		}
		LightmapSettings.lightmaps = m_Lightmaps.Select((LightmapData ld) => new UnityEngine.LightmapData
		{
			lightmapColor = ld.LightmapColor,
			lightmapDir = ld.LightmapDir,
			shadowMask = (ShadowmaskDisabled ? null : ld.ShadowMask)
		}).ToArray();
		try
		{
			if ((bool)m_LightProbes)
			{
				LightmapSettings.lightProbes = m_LightProbes;
			}
			if (LightmapSettings.lightProbes != null)
			{
				if (LightmapSettings.lightProbes.count == m_BakedLightProbes.Length)
				{
					SphericalHarmonicsL2[] bakedProbes = LightmapSettings.lightProbes.bakedProbes;
					for (int j = 0; j < bakedProbes.Length; j++)
					{
						bakedProbes[j] = ConvertSphericalHarmonics(m_BakedLightProbes[j]);
					}
					LightmapSettings.lightProbes.bakedProbes = bakedProbes;
				}
				else
				{
					PFLog.Default.Error("Different lightprobes count: in scene {0}, in storage {1}", LightmapSettings.lightProbes.count, m_BakedLightProbes.Length);
				}
			}
			else
			{
				PFLog.Default.Error("LightmapSettings.lightProbes is null");
			}
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogError("Can't load light probes!");
			UnityEngine.Debug.LogException(exception);
		}
		LightmapStorage.LightmapSettingsChanged?.Invoke();
	}

	private void ApplyLightmapSettings(Chunk chunk)
	{
		LightmapRenderersData lightmapRenderersData = m_RendererData.FirstOrDefault((LightmapRenderersData d) => d.Id == chunk.RelatedRenderersDataId);
		if (!lightmapRenderersData)
		{
			PFLog.Default.Error("Can't find renderers data");
			return;
		}
		if (lightmapRenderersData.Renderers.Length != chunk.Renderers.Length)
		{
			PFLog.Default.Error("Invalid renderers data, lightmap settings will not be applied");
			return;
		}
		int num = 0;
		for (int i = 0; i < chunk.Renderers.Length; i++)
		{
			RendererData rendererData = chunk.Renderers[i];
			Renderer renderer = lightmapRenderersData.Renderers[i];
			if (!renderer)
			{
				num++;
				continue;
			}
			renderer.lightmapIndex = rendererData.LightmapIndex;
			renderer.lightmapScaleOffset = rendererData.ScaleOffset;
		}
		if (num > 0)
		{
			PFLog.Default.Warning($"{num} renderers are missing (it can be consequence of automatic scenes' convertation)");
		}
	}

	private static SphericalHarmonicsL2 ConvertSphericalHarmonics(SphericalHarmonics sph)
	{
		SphericalHarmonicsL2 result = default(SphericalHarmonicsL2);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 9; j++)
			{
				result[i, j] = sph.Coefficients[i * 9 + j];
			}
		}
		return result;
	}

	public LightmapRenderersData GetRendererData(Renderer renderer, out int index)
	{
		LightmapRenderersData[] array = m_RendererData.EmptyIfNull();
		foreach (LightmapRenderersData lightmapRenderersData in array)
		{
			index = Array.IndexOf(lightmapRenderersData.Renderers, renderer);
			if (index >= 0)
			{
				return lightmapRenderersData;
			}
		}
		index = -1;
		return null;
	}

	public Renderer GetRendererByData(string dataId, int rendererIndex)
	{
		LightmapRenderersData[] array = m_RendererData.EmptyIfNull();
		foreach (LightmapRenderersData lightmapRenderersData in array)
		{
			if (lightmapRenderersData.Id == dataId)
			{
				Renderer renderer = lightmapRenderersData.Renderers.Get(rendererIndex);
				if (renderer == null)
				{
					PFLog.Default.Error($"No renderer found in lightmap storage for {dataId} #{rendererIndex}");
					return null;
				}
				return renderer;
			}
		}
		return null;
	}

	private void OnEnable()
	{
		Instances.Add(this);
	}

	private void OnDisable()
	{
		Instances.Remove(this);
	}
}
