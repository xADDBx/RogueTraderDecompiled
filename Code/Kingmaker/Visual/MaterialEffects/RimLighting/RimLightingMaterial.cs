using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.RimLighting;

public readonly struct RimLightingMaterial
{
	public struct Snapshot
	{
		public float RimLighting;

		public Color RimColor;

		public float RimPower;
	}

	private static readonly int _RimLighting = Shader.PropertyToID("_RimLighting");

	private static readonly int _RimColor = Shader.PropertyToID("_RimColor");

	private static readonly int _RimPower = Shader.PropertyToID("_RimPower");

	private static readonly Dictionary<Shader, bool> s_ShadersCompatibilityCache = new Dictionary<Shader, bool>();

	private readonly Material m_Material;

	public float RimLighting
	{
		get
		{
			return m_Material.GetFloat(_RimLighting);
		}
		set
		{
			m_Material.SetFloat(_RimLighting, value);
		}
	}

	public Color RimColor
	{
		get
		{
			return m_Material.GetColor(_RimColor);
		}
		set
		{
			m_Material.SetColor(_RimColor, value);
		}
	}

	public float RimPower
	{
		get
		{
			return m_Material.GetFloat(_RimPower);
		}
		set
		{
			m_Material.SetFloat(_RimPower, value);
		}
	}

	public RimLightingMaterial(Material material)
	{
		this = default(RimLightingMaterial);
		m_Material = material;
	}

	public Snapshot TakeSnapshot()
	{
		Snapshot result = default(Snapshot);
		result.RimLighting = RimLighting;
		result.RimColor = RimColor;
		result.RimPower = RimPower;
		return result;
	}

	public void ApplySnapshot(in Snapshot snapshot)
	{
		RimLighting = snapshot.RimLighting;
		RimColor = snapshot.RimColor;
		RimPower = snapshot.RimPower;
	}

	public static bool IsMaterialCompatible(Material material)
	{
		Shader shader = material.shader;
		if (!s_ShadersCompatibilityCache.TryGetValue(shader, out var value))
		{
			value = material.HasFloat(_RimLighting) && material.HasColor(_RimColor) && material.HasFloat(_RimPower);
			s_ShadersCompatibilityCache.Add(shader, value);
		}
		return value;
	}
}
