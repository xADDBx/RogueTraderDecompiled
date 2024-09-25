using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.ColorTint;

public readonly struct ColorTintMaterial
{
	public struct Snapshot
	{
		public Color BaseColor;

		public float BaseColorBlending;
	}

	private static readonly int _BaseColor = Shader.PropertyToID("_BaseColor");

	private static readonly int _BaseColorBlending = Shader.PropertyToID("_BaseColorBlending");

	private static readonly Dictionary<Shader, bool> s_ShadersCompatibilityCache = new Dictionary<Shader, bool>();

	private readonly Material m_Material;

	public Color BaseColor
	{
		get
		{
			return m_Material.GetColor(_BaseColor);
		}
		set
		{
			m_Material.SetColor(_BaseColor, value);
		}
	}

	public float BaseColorBlending
	{
		get
		{
			return m_Material.GetFloat(_BaseColorBlending);
		}
		set
		{
			m_Material.SetFloat(_BaseColorBlending, value);
		}
	}

	public ColorTintMaterial(Material material)
	{
		m_Material = material;
	}

	public Snapshot TakeSnapshot()
	{
		Snapshot result = default(Snapshot);
		result.BaseColor = BaseColor;
		result.BaseColorBlending = BaseColorBlending;
		return result;
	}

	public void ApplySnapshot(in Snapshot snapshot)
	{
		BaseColor = snapshot.BaseColor;
		BaseColorBlending = snapshot.BaseColorBlending;
	}

	public static bool IsMaterialCompatible(Material material)
	{
		Shader shader = material.shader;
		if (!s_ShadersCompatibilityCache.TryGetValue(shader, out var value))
		{
			value = material.HasColor(_BaseColor) && material.HasFloat(_BaseColorBlending);
			s_ShadersCompatibilityCache.Add(shader, value);
		}
		return value;
	}
}
