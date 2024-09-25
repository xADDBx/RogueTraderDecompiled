using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Owlcat.Runtime.UI.Controls.SelectableState;

[Serializable]
public struct OwlcatSelectableActiveBlock : IEquatable<OwlcatSelectableActiveBlock>
{
	[SerializeField]
	private bool m_NormalState;

	[SerializeField]
	private bool m_HighlightedState;

	[FormerlySerializedAs("m_SelectedState")]
	[SerializeField]
	private bool m_FocusedState;

	[SerializeField]
	private bool m_PressedState;

	[SerializeField]
	private bool m_DisabledState;

	public bool Normal
	{
		get
		{
			return m_NormalState;
		}
		set
		{
			m_NormalState = value;
		}
	}

	public bool Highlighted
	{
		get
		{
			return m_HighlightedState;
		}
		set
		{
			m_HighlightedState = value;
		}
	}

	public bool Selected
	{
		get
		{
			return m_FocusedState;
		}
		set
		{
			m_FocusedState = value;
		}
	}

	public bool Pressed
	{
		get
		{
			return m_PressedState;
		}
		set
		{
			m_PressedState = value;
		}
	}

	public bool Disabled
	{
		get
		{
			return m_DisabledState;
		}
		set
		{
			m_DisabledState = value;
		}
	}

	public static OwlcatSelectableActiveBlock DefaultActiveBlock
	{
		get
		{
			OwlcatSelectableActiveBlock result = default(OwlcatSelectableActiveBlock);
			result.Normal = true;
			result.Highlighted = true;
			result.Pressed = true;
			result.Disabled = true;
			return result;
		}
	}

	public bool Equals(OwlcatSelectableActiveBlock block)
	{
		if (m_NormalState == block.Normal && m_HighlightedState == block.Highlighted && m_PressedState == block.Pressed)
		{
			return m_DisabledState == block.Disabled;
		}
		return false;
	}
}
