using System;
using UnityEngine;

namespace Owlcat.Runtime.UI.Controls.SelectableState;

[Serializable]
public struct OwlcatSelectableSpriteSwapBlock : IEquatable<OwlcatSelectableSpriteSwapBlock>
{
	[SerializeField]
	private Sprite m_NormalSprite;

	[SerializeField]
	private Sprite m_HighlightedSprite;

	[SerializeField]
	private Sprite m_PressedSprite;

	[SerializeField]
	private Sprite m_FocusedSprite;

	[SerializeField]
	private Sprite m_DisabledSprite;

	public Sprite normalSprite
	{
		get
		{
			return m_NormalSprite;
		}
		set
		{
			m_NormalSprite = value;
		}
	}

	public Sprite highlightedSprite
	{
		get
		{
			return m_HighlightedSprite;
		}
		set
		{
			m_HighlightedSprite = value;
		}
	}

	public Sprite pressedSprite
	{
		get
		{
			return m_PressedSprite;
		}
		set
		{
			m_PressedSprite = value;
		}
	}

	public Sprite focusedSprite
	{
		get
		{
			return m_FocusedSprite;
		}
		set
		{
			m_FocusedSprite = value;
		}
	}

	public Sprite disabledSprite
	{
		get
		{
			return m_DisabledSprite;
		}
		set
		{
			m_DisabledSprite = value;
		}
	}

	public bool Equals(OwlcatSelectableSpriteSwapBlock other)
	{
		if (normalSprite == other.normalSprite && highlightedSprite == other.highlightedSprite && pressedSprite == other.pressedSprite && focusedSprite == other.focusedSprite)
		{
			return disabledSprite == other.disabledSprite;
		}
		return false;
	}
}
