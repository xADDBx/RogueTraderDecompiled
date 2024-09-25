using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Visual.MaterialEffects.LayeredMaterial;

internal sealed class MaterialPropertiesSnapshot
{
	private readonly Dictionary<int, float> m_FloatMap = new Dictionary<int, float>();

	private readonly Dictionary<int, Color> m_ColorMap = new Dictionary<int, Color>();

	private readonly Dictionary<int, Texture> m_TextureMap = new Dictionary<int, Texture>();

	public void Capture(Material material)
	{
		Clear();
		if (material == null)
		{
			return;
		}
		Shader shader = material.shader;
		if (shader == null)
		{
			return;
		}
		int i = 0;
		for (int propertyCount = shader.GetPropertyCount(); i < propertyCount; i++)
		{
			switch (shader.GetPropertyType(i))
			{
			case ShaderPropertyType.Float:
			case ShaderPropertyType.Range:
			case ShaderPropertyType.Int:
			{
				int propertyNameId3 = shader.GetPropertyNameId(i);
				m_FloatMap.Add(propertyNameId3, material.GetFloat(propertyNameId3));
				break;
			}
			case ShaderPropertyType.Color:
			{
				int propertyNameId2 = shader.GetPropertyNameId(i);
				m_ColorMap.Add(propertyNameId2, material.GetColor(propertyNameId2));
				break;
			}
			case ShaderPropertyType.Texture:
			{
				int propertyNameId = shader.GetPropertyNameId(i);
				m_TextureMap.Add(propertyNameId, material.GetTexture(propertyNameId));
				break;
			}
			}
		}
	}

	public void Clear()
	{
		m_FloatMap.Clear();
		m_ColorMap.Clear();
		m_TextureMap.Clear();
	}

	public bool TryGetFloat(int nameId, out float value)
	{
		return m_FloatMap.TryGetValue(nameId, out value);
	}

	public bool TryGetColor(int nameId, out Color value)
	{
		return m_ColorMap.TryGetValue(nameId, out value);
	}

	public bool TryGetTexture(int nameId, out Texture value)
	{
		return m_TextureMap.TryGetValue(nameId, out value);
	}
}
