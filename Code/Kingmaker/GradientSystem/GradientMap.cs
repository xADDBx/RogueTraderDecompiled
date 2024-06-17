using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.GradientSystem;

public class GradientMap
{
	private readonly Dictionary<GradientWrapper, Texture2D> m_Map = new Dictionary<GradientWrapper, Texture2D>();

	public Texture2D GetTextureFromGradient(Gradient gradient, GradientSystem.Mode mode, out bool isNewTexure)
	{
		GradientWrapper key = new GradientWrapper(gradient);
		isNewTexure = !m_Map.ContainsKey(key);
		if (!isNewTexure)
		{
			return m_Map[key];
		}
		Texture2D texture2D = GradientSystem.GenerateTextureFromGradient(gradient);
		m_Map.Add(key, texture2D);
		return texture2D;
	}

	public void Clear()
	{
		foreach (KeyValuePair<GradientWrapper, Texture2D> item in m_Map)
		{
			Object.Destroy(item.Value);
		}
		m_Map.Clear();
	}

	public void ReloadFromFiles()
	{
	}
}
