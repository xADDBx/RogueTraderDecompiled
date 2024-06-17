using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.UI.Common;

public class LayoutRedirectElement : UIBehaviour, ILayoutElement
{
	public enum Source
	{
		None,
		Min,
		Prefered
	}

	public enum Axis
	{
		Horizontal,
		Vertical
	}

	[SerializeField]
	protected UnityEngine.Object m_Source;

	[SerializeField]
	protected Source m_MinWidth;

	[SerializeField]
	protected Source m_MinHeight;

	[SerializeField]
	protected Source m_PreferredWidth;

	[SerializeField]
	protected Source m_PreferredHeight;

	[SerializeField]
	protected int m_LayoutPriority = 1;

	[NonSerialized]
	private RectTransform m_Rect;

	public float minWidth { get; protected set; }

	public float preferredWidth { get; protected set; }

	public float flexibleWidth => -1f;

	public float minHeight { get; protected set; }

	public float preferredHeight { get; protected set; }

	public float flexibleHeight => -1f;

	public int layoutPriority => m_LayoutPriority;

	private RectTransform rectTransform
	{
		get
		{
			if (m_Rect == null)
			{
				m_Rect = GetComponent<RectTransform>();
			}
			return m_Rect;
		}
	}

	protected ILayoutElement GetSourceLayout()
	{
		if (!(m_Source != null))
		{
			return null;
		}
		return m_Source as ILayoutElement;
	}

	public float GetSize(Axis axis, Source source)
	{
		float num = -1f;
		if (axis == Axis.Horizontal)
		{
			return (source != Source.Min) ? (GetSourceLayout()?.preferredWidth ?? (-1f)) : (GetSourceLayout()?.minWidth ?? (-1f));
		}
		return (source != Source.Min) ? (GetSourceLayout()?.preferredHeight ?? (-1f)) : (GetSourceLayout()?.minHeight ?? (-1f));
	}

	public void CalculateLayoutInputHorizontal()
	{
		minWidth = GetSize(Axis.Horizontal, m_MinWidth);
		preferredWidth = GetSize(Axis.Horizontal, m_PreferredWidth);
	}

	public void CalculateLayoutInputVertical()
	{
		minHeight = GetSize(Axis.Vertical, m_MinHeight);
		preferredHeight = GetSize(Axis.Vertical, m_PreferredHeight);
	}
}
