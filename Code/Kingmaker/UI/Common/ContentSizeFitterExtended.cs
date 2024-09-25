using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.UI.Common;

[AddComponentMenu("Layout/Content Size Fitter", 141)]
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class ContentSizeFitterExtended : UIBehaviour, ILayoutSelfController, ILayoutController
{
	public enum FitMode
	{
		Unconstrained,
		MinSize,
		PreferredSize,
		Clamp
	}

	[SerializeField]
	protected FitMode m_HorizontalFit;

	[SerializeField]
	protected FitMode m_VerticalFit;

	[NonSerialized]
	private RectTransform m_Rect;

	private DrivenRectTransformTracker m_Tracker;

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

	protected ContentSizeFitterExtended()
	{
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		SetDirty();
	}

	protected override void OnDisable()
	{
		m_Tracker.Clear();
		LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		base.OnDisable();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		SetDirty();
	}

	private void HandleSelfFittingAlongAxis(int axis)
	{
		FitMode fitMode = ((axis == 0) ? m_HorizontalFit : m_VerticalFit);
		if (fitMode == FitMode.Unconstrained)
		{
			m_Tracker.Add(this, rectTransform, DrivenTransformProperties.None);
			return;
		}
		m_Tracker.Add(this, rectTransform, (axis == 0) ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY);
		switch (fitMode)
		{
		case FitMode.MinSize:
			rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetMinSize(m_Rect, axis));
			return;
		case FitMode.PreferredSize:
			rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetPreferredSize(m_Rect, axis));
			return;
		}
		if (m_Rect.parent is RectTransform)
		{
			float num = (m_Rect.parent as RectTransform).rect.size[axis];
			float preferredSize = LayoutUtility.GetPreferredSize(m_Rect, axis);
			rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, (preferredSize > num) ? num : preferredSize);
		}
	}

	public virtual void SetLayoutHorizontal()
	{
		m_Tracker.Clear();
		HandleSelfFittingAlongAxis(0);
	}

	public virtual void SetLayoutVertical()
	{
		HandleSelfFittingAlongAxis(1);
	}

	protected void SetDirty()
	{
		if (IsActive())
		{
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}
	}
}
