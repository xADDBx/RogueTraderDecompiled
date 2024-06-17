using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.AdditionalAlbedo;

public readonly struct AdditionalAlbedoMaterial
{
	public struct Snapshot
	{
		public bool AdditionalAlbedoEnabled;

		public Texture AdditionalAlbedoMap;

		public Vector4 AdditionalAlbedoMap_ST;

		public float AdditionalAlbedoFactor;

		public Color AdditionalAlbedoColor;

		public float AdditionalAlbedoColorScale;

		public float AdditionalAlbedoAlphaScale;

		public Vector4 DissolveMap_ST;
	}

	private const string ADDITIONAL_ALBEDO = "ADDITIONAL_ALBEDO";

	private static readonly int _AdditionalAlbedoEnabled = Shader.PropertyToID("_AdditionalAlbedoEnabled");

	private static readonly int _AdditionalAlbedoMap = Shader.PropertyToID("_AdditionalAlbedoMap");

	private static readonly int _AdditionalAlbedoMap_ST = Shader.PropertyToID("_AdditionalAlbedoMap_ST");

	private static readonly int _AdditionalAlbedoFactor = Shader.PropertyToID("_AdditionalAlbedoFactor");

	private static readonly int _AdditionalAlbedoColor = Shader.PropertyToID("_AdditionalAlbedoColor");

	private static readonly int _AdditionalAlbedoColorScale = Shader.PropertyToID("_AdditionalAlbedoColorScale");

	private static readonly int _AdditionalAlbedoAlphaScale = Shader.PropertyToID("_AdditionalAlbedoAlphaScale");

	private static readonly int _DissolveMap_ST = Shader.PropertyToID("_DissolveMap_ST");

	private static readonly Dictionary<Shader, bool> s_ShadersCompatibilityCache = new Dictionary<Shader, bool>();

	private readonly Material m_Material;

	public bool AdditionalAlbedoEnabled
	{
		get
		{
			return m_Material.IsKeywordEnabled("ADDITIONAL_ALBEDO");
		}
		set
		{
			if (value)
			{
				m_Material.EnableKeyword("ADDITIONAL_ALBEDO");
				m_Material.SetFloat(_AdditionalAlbedoEnabled, 1f);
			}
			else
			{
				m_Material.DisableKeyword("ADDITIONAL_ALBEDO");
				m_Material.SetFloat(_AdditionalAlbedoEnabled, 0f);
			}
		}
	}

	public Texture AdditionalAlbedoMap
	{
		get
		{
			return m_Material.GetTexture(_AdditionalAlbedoMap);
		}
		set
		{
			m_Material.SetTexture(_AdditionalAlbedoMap, value);
		}
	}

	public Vector4 AdditionalAlbedoMap_ST
	{
		get
		{
			return m_Material.GetVector(_AdditionalAlbedoMap_ST);
		}
		set
		{
			m_Material.SetVector(_AdditionalAlbedoMap_ST, value);
		}
	}

	public float AdditionalAlbedoFactor
	{
		get
		{
			return m_Material.GetFloat(_AdditionalAlbedoFactor);
		}
		set
		{
			m_Material.SetFloat(_AdditionalAlbedoFactor, value);
		}
	}

	public Color AdditionalAlbedoColor
	{
		get
		{
			return m_Material.GetColor(_AdditionalAlbedoColor);
		}
		set
		{
			m_Material.SetColor(_AdditionalAlbedoColor, value);
		}
	}

	public float AdditionalAlbedoColorScale
	{
		get
		{
			return m_Material.GetFloat(_AdditionalAlbedoColorScale);
		}
		set
		{
			m_Material.SetFloat(_AdditionalAlbedoColorScale, value);
		}
	}

	public float AdditionalAlbedoAlphaScale
	{
		get
		{
			return m_Material.GetFloat(_AdditionalAlbedoAlphaScale);
		}
		set
		{
			m_Material.SetFloat(_AdditionalAlbedoAlphaScale, value);
		}
	}

	public AdditionalAlbedoMaterial(Material material)
	{
		m_Material = material;
	}

	public Snapshot TakeSnapshot()
	{
		Snapshot result = default(Snapshot);
		result.AdditionalAlbedoEnabled = AdditionalAlbedoEnabled;
		result.AdditionalAlbedoMap = AdditionalAlbedoMap;
		result.AdditionalAlbedoMap_ST = AdditionalAlbedoMap_ST;
		result.AdditionalAlbedoFactor = AdditionalAlbedoFactor;
		result.AdditionalAlbedoColor = AdditionalAlbedoColor;
		result.AdditionalAlbedoColorScale = AdditionalAlbedoColorScale;
		result.AdditionalAlbedoAlphaScale = AdditionalAlbedoAlphaScale;
		result.DissolveMap_ST = m_Material.GetVector(_DissolveMap_ST);
		return result;
	}

	public void ApplySnapshot(in Snapshot snapshot)
	{
		AdditionalAlbedoEnabled = snapshot.AdditionalAlbedoEnabled;
		AdditionalAlbedoMap = snapshot.AdditionalAlbedoMap;
		AdditionalAlbedoMap_ST = snapshot.AdditionalAlbedoMap_ST;
		AdditionalAlbedoFactor = snapshot.AdditionalAlbedoFactor;
		AdditionalAlbedoColor = snapshot.AdditionalAlbedoColor;
		AdditionalAlbedoColorScale = snapshot.AdditionalAlbedoColorScale;
		AdditionalAlbedoAlphaScale = snapshot.AdditionalAlbedoAlphaScale;
	}

	public static bool IsMaterialCompatible(Material material)
	{
		Shader shader = material.shader;
		if (!s_ShadersCompatibilityCache.TryGetValue(shader, out var value))
		{
			value = material.shader.keywordSpace.FindKeyword("ADDITIONAL_ALBEDO").isValid && material.HasFloat(_AdditionalAlbedoEnabled) && material.HasTexture(_AdditionalAlbedoMap) && material.HasVector(_AdditionalAlbedoMap_ST) && material.HasFloat(_AdditionalAlbedoFactor) && material.HasColor(_AdditionalAlbedoColor) && material.HasFloat(_AdditionalAlbedoColorScale) && material.HasFloat(_AdditionalAlbedoAlphaScale) && material.HasVector(_DissolveMap_ST);
			s_ShadersCompatibilityCache.Add(shader, value);
		}
		return value;
	}
}
