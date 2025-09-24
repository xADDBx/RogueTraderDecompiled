using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
public class WaaaghPipelineGlobalSettings : RenderPipelineGlobalSettings
{
	[SerializeField]
	private int k_AssetVersion = 1;

	[SerializeField]
	private int k_AssetPreviousVersion = 1;

	private static WaaaghPipelineGlobalSettings s_CachedInstance = null;

	[SerializeReference]
	private List<IRenderPipelineGraphicsSettings> m_Settings = new List<IRenderPipelineGraphicsSettings>();

	public static readonly string DefaultAssetName = "WaaaghPipelineGlobalSettings";

	[NonSerialized]
	private string[] m_RenderingLayerNames;

	[NonSerialized]
	private string[] m_PrefixedRenderingLayerNames;

	[NonSerialized]
	private string[] m_PrefixedLightLayerNames;

	private static readonly string[] k_DefaultLightLayerNames = new string[8] { "Light Layer default", "Light Layer 1", "Light Layer 2", "Light Layer 3", "Light Layer 4", "Light Layer 5", "Light Layer 6", "Light Layer 7" };

	public string lightLayerName0 = k_DefaultLightLayerNames[0];

	public string lightLayerName1 = k_DefaultLightLayerNames[1];

	public string lightLayerName2 = k_DefaultLightLayerNames[2];

	public string lightLayerName3 = k_DefaultLightLayerNames[3];

	public string lightLayerName4 = k_DefaultLightLayerNames[4];

	public string lightLayerName5 = k_DefaultLightLayerNames[5];

	public string lightLayerName6 = k_DefaultLightLayerNames[6];

	public string lightLayerName7 = k_DefaultLightLayerNames[7];

	[NonSerialized]
	private string[] m_LightLayerNames;

	public bool SupportRuntimeDebugDisplay;

	public static WaaaghPipelineGlobalSettings Instance
	{
		get
		{
			if (s_CachedInstance == null)
			{
				s_CachedInstance = GraphicsSettings.GetSettingsForRenderPipeline<WaaaghPipeline>() as WaaaghPipelineGlobalSettings;
			}
			return s_CachedInstance;
		}
	}

	protected override List<IRenderPipelineGraphicsSettings> settingsList => m_Settings;

	private string[] renderingLayerNames
	{
		get
		{
			if (m_RenderingLayerNames == null)
			{
				UpdateRenderingLayerNames();
			}
			return m_RenderingLayerNames;
		}
	}

	private string[] prefixedRenderingLayerNames
	{
		get
		{
			if (m_PrefixedRenderingLayerNames == null)
			{
				UpdateRenderingLayerNames();
			}
			return m_PrefixedRenderingLayerNames;
		}
	}

	public string[] renderingLayerMaskNames => renderingLayerNames;

	public string[] prefixedRenderingLayerMaskNames => prefixedRenderingLayerNames;

	public string[] prefixedLightLayerNames
	{
		get
		{
			if (m_PrefixedLightLayerNames == null)
			{
				UpdateRenderingLayerNames();
			}
			return m_PrefixedLightLayerNames;
		}
	}

	public string[] lightLayerNames
	{
		get
		{
			if (m_LightLayerNames == null)
			{
				m_LightLayerNames = new string[8];
			}
			m_LightLayerNames[0] = lightLayerName0;
			m_LightLayerNames[1] = lightLayerName1;
			m_LightLayerNames[2] = lightLayerName2;
			m_LightLayerNames[3] = lightLayerName3;
			m_LightLayerNames[4] = lightLayerName4;
			m_LightLayerNames[5] = lightLayerName5;
			m_LightLayerNames[6] = lightLayerName6;
			m_LightLayerNames[7] = lightLayerName7;
			return m_LightLayerNames;
		}
	}

	public new void OnAfterDeserialize()
	{
	}

	public void GetAllSettings(List<IRenderPipelineGraphicsSettings> settings)
	{
		settings.AddRange(settingsList);
	}

	private void Reset()
	{
		UpdateRenderingLayerNames();
	}

	internal void UpdateRenderingLayerNames()
	{
		if (m_RenderingLayerNames == null)
		{
			m_RenderingLayerNames = new string[32];
		}
		int num = 0;
		m_RenderingLayerNames[num++] = lightLayerName0;
		m_RenderingLayerNames[num++] = lightLayerName1;
		m_RenderingLayerNames[num++] = lightLayerName2;
		m_RenderingLayerNames[num++] = lightLayerName3;
		m_RenderingLayerNames[num++] = lightLayerName4;
		m_RenderingLayerNames[num++] = lightLayerName5;
		m_RenderingLayerNames[num++] = lightLayerName6;
		m_RenderingLayerNames[num++] = lightLayerName7;
		for (int i = num; i < m_RenderingLayerNames.Length; i++)
		{
			m_RenderingLayerNames[i] = $"Unused {i}";
		}
		if (m_PrefixedRenderingLayerNames == null)
		{
			m_PrefixedRenderingLayerNames = new string[32];
		}
		if (m_PrefixedLightLayerNames == null)
		{
			m_PrefixedLightLayerNames = new string[8];
		}
		for (int j = 0; j < m_PrefixedRenderingLayerNames.Length; j++)
		{
			m_PrefixedRenderingLayerNames[j] = $"{j}: {m_RenderingLayerNames[j]}";
			if (j < 8)
			{
				m_PrefixedLightLayerNames[j] = m_PrefixedRenderingLayerNames[j];
			}
		}
	}

	internal void ResetRenderingLayerNames()
	{
		lightLayerName0 = k_DefaultLightLayerNames[0];
		lightLayerName1 = k_DefaultLightLayerNames[1];
		lightLayerName2 = k_DefaultLightLayerNames[2];
		lightLayerName3 = k_DefaultLightLayerNames[3];
		lightLayerName4 = k_DefaultLightLayerNames[4];
		lightLayerName5 = k_DefaultLightLayerNames[5];
		lightLayerName6 = k_DefaultLightLayerNames[6];
		lightLayerName7 = k_DefaultLightLayerNames[7];
	}
}
