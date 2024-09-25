using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.MaterialParametersOverride;

public readonly struct ParametersOverrideMaterial
{
	public struct Snapshot
	{
		public bool NormalMapKeywordEnabled;

		public bool SpecularHighlightsOffKeywordEnabled;

		public bool EmissionKeywordEnabled;

		public float Roughness;

		public float Metallic;

		public float EmissionColorScale;

		public Texture BaseMap;

		public Texture BumpMap;

		public Vector4 BaseMap_ST;

		public Vector4 AdditionalAlbedoMap_ST;
	}

	private const string _NORMALMAP = "_NORMALMAP";

	private const string _SPECULARHIGHLIGHTS_OFF = "_SPECULARHIGHLIGHTS_OFF";

	private const string _EMISSION = "_EMISSION";

	private static readonly int _Emission = Shader.PropertyToID("_Emission");

	private static readonly int _Roughness = Shader.PropertyToID("_Roughness");

	private static readonly int _Metallic = Shader.PropertyToID("_Metallic");

	private static readonly int _EmissionColorScale = Shader.PropertyToID("_EmissionColorScale");

	private static readonly int _BumpMap = Shader.PropertyToID("_BumpMap");

	private static readonly int _BaseMap = Shader.PropertyToID("_BaseMap");

	private static readonly int _BaseMap_ST = Shader.PropertyToID("_BaseMap_ST");

	private static readonly int _AdditionalAlbedoMap_ST = Shader.PropertyToID("_AdditionalAlbedoMap_ST");

	private static readonly Dictionary<Shader, bool> s_ShadersCompatibilityCache = new Dictionary<Shader, bool>();

	private readonly Material m_Material;

	public bool NormalMapKeywordEnabled
	{
		get
		{
			return m_Material.IsKeywordEnabled("_NORMALMAP");
		}
		set
		{
			m_Material.EnableKeyword("_NORMALMAP");
		}
	}

	public bool SpecularHighlightsOffKeywordEnabled
	{
		get
		{
			return m_Material.IsKeywordEnabled("_SPECULARHIGHLIGHTS_OFF");
		}
		set
		{
			m_Material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
		}
	}

	public bool EmissionKeywordEnabled
	{
		get
		{
			return m_Material.IsKeywordEnabled("_EMISSION");
		}
		set
		{
			if (value)
			{
				m_Material.EnableKeyword("_EMISSION");
				m_Material.SetFloat(_Emission, 1f);
			}
			else
			{
				m_Material.DisableKeyword("_EMISSION");
				m_Material.SetFloat(_Emission, 0f);
			}
		}
	}

	public float Roughness
	{
		get
		{
			return m_Material.GetFloat(_Roughness);
		}
		set
		{
			m_Material.SetFloat(_Roughness, value);
		}
	}

	public float Metallic
	{
		get
		{
			return m_Material.GetFloat(_Metallic);
		}
		set
		{
			m_Material.SetFloat(_Metallic, value);
		}
	}

	public float EmissionColorScale
	{
		get
		{
			return m_Material.GetFloat(_EmissionColorScale);
		}
		set
		{
			m_Material.SetFloat(_EmissionColorScale, value);
		}
	}

	public Texture BaseMap
	{
		get
		{
			return m_Material.GetTexture(_BaseMap);
		}
		set
		{
			m_Material.SetTexture(_BaseMap, value);
		}
	}

	public Texture BumpMap
	{
		get
		{
			return m_Material.GetTexture(_BumpMap);
		}
		set
		{
			m_Material.SetTexture(_BumpMap, value);
		}
	}

	public Vector4 BaseMap_ST
	{
		get
		{
			return m_Material.GetVector(_BaseMap_ST);
		}
		set
		{
			m_Material.SetVector(_BaseMap_ST, value);
		}
	}

	public ParametersOverrideMaterial(Material material)
	{
		m_Material = material;
	}

	public Snapshot TakeSnapshot()
	{
		Snapshot result = default(Snapshot);
		result.NormalMapKeywordEnabled = NormalMapKeywordEnabled;
		result.SpecularHighlightsOffKeywordEnabled = SpecularHighlightsOffKeywordEnabled;
		result.EmissionKeywordEnabled = EmissionKeywordEnabled;
		result.Roughness = Roughness;
		result.Metallic = Metallic;
		result.EmissionColorScale = EmissionColorScale;
		result.BaseMap = BaseMap;
		result.BumpMap = BumpMap;
		result.BaseMap_ST = BaseMap_ST;
		result.AdditionalAlbedoMap_ST = m_Material.GetVector(_AdditionalAlbedoMap_ST);
		return result;
	}

	public void ApplySnapshot(Snapshot snapshot)
	{
		NormalMapKeywordEnabled = snapshot.NormalMapKeywordEnabled;
		SpecularHighlightsOffKeywordEnabled = snapshot.SpecularHighlightsOffKeywordEnabled;
		EmissionKeywordEnabled = snapshot.EmissionKeywordEnabled;
		Roughness = snapshot.Roughness;
		Metallic = snapshot.Metallic;
		EmissionColorScale = snapshot.EmissionColorScale;
		BaseMap = snapshot.BaseMap;
		BumpMap = snapshot.BumpMap;
		BaseMap_ST = snapshot.BaseMap_ST;
	}

	public static bool IsMaterialCompatible(Material material)
	{
		Shader shader = material.shader;
		if (!s_ShadersCompatibilityCache.TryGetValue(shader, out var value))
		{
			value = shader.keywordSpace.FindKeyword("_NORMALMAP").isValid && shader.keywordSpace.FindKeyword("_SPECULARHIGHLIGHTS_OFF").isValid && shader.keywordSpace.FindKeyword("_EMISSION").isValid && material.HasFloat(_Emission) && material.HasFloat(_Roughness) && material.HasFloat(_Metallic) && material.HasFloat(_EmissionColorScale) && material.HasTexture(_BaseMap) && material.HasTexture(_BumpMap) && material.HasVector(_BaseMap_ST) && material.HasVector(_AdditionalAlbedoMap_ST);
			s_ShadersCompatibilityCache.Add(shader, value);
		}
		return value;
	}
}
