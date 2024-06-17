using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Owlcat.Runtime.UI.Controls.SelectableState;

[Serializable]
public struct OwlcatSelectableCanvasGroupBlock : IEquatable<OwlcatSelectableCanvasGroupBlock>
{
	[SerializeField]
	[Range(0f, 1f)]
	private float m_NormalState;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_HighlightedState;

	[FormerlySerializedAs("m_SelectedState")]
	[SerializeField]
	[Range(0f, 1f)]
	private float m_FocusedState;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_PressedState;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_DisabledState;

	[SerializeField]
	private float m_FadeDuration;

	public float Normal
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

	public float Highlighted
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

	public float Selected
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

	public float Pressed
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

	public float Disabled
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

	public float FadeDuration
	{
		get
		{
			return m_FadeDuration;
		}
		set
		{
			m_FadeDuration = value;
		}
	}

	public static OwlcatSelectableCanvasGroupBlock DefaultActiveBlock
	{
		get
		{
			OwlcatSelectableCanvasGroupBlock result = default(OwlcatSelectableCanvasGroupBlock);
			result.Normal = 1f;
			result.Highlighted = 1f;
			result.Pressed = 1f;
			result.Disabled = 1f;
			result.FadeDuration = 0.1f;
			return result;
		}
	}

	public bool Equals(OwlcatSelectableCanvasGroupBlock block)
	{
		if (Math.Abs(m_NormalState - block.Normal) < Mathf.Epsilon && Math.Abs(m_HighlightedState - block.Highlighted) < Mathf.Epsilon && Math.Abs(m_PressedState - block.Pressed) < Mathf.Epsilon && Math.Abs(m_DisabledState - block.Disabled) < Mathf.Epsilon)
		{
			return Math.Abs(m_FadeDuration - block.FadeDuration) < Mathf.Epsilon;
		}
		return false;
	}
}
