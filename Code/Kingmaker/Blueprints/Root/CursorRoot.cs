using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

public class CursorRoot : ScriptableObject
{
	[SerializeField]
	private List<CursorEntry> m_CursorEntries;

	public Sprite GetSprite(CursorType type)
	{
		return m_CursorEntries.FirstOrDefault((CursorEntry e) => e.Type == type)?.Sprite;
	}

	public Texture2D GetTexture(CursorType type)
	{
		return m_CursorEntries.FirstOrDefault((CursorEntry e) => e.Type == type)?.Texture;
	}
}
