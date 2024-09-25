using System;
using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Terrain;

[Serializable]
public class TerrainTexturesConfig
{
	public enum TextureType
	{
		Diffuse,
		Normal,
		Masks
	}

	private Dictionary<Texture2D, int> m_TextureIndexMap = new Dictionary<Texture2D, int>();

	public List<Texture2D> TexturesInArray = new List<Texture2D>();

	public Texture2DArray Texture2DArray;

	public void RefreshIndexMap()
	{
		m_TextureIndexMap.Clear();
		for (int i = 0; i < TexturesInArray.Count; i++)
		{
			m_TextureIndexMap[TexturesInArray[i]] = i;
		}
	}

	public int GetIndex(Texture2D texture)
	{
		return m_TextureIndexMap[texture];
	}
}
