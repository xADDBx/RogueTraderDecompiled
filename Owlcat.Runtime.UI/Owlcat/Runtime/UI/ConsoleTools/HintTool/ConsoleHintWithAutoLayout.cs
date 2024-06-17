using System;
using UnityEngine;
using UnityEngine.UI;

namespace Owlcat.Runtime.UI.ConsoleTools.HintTool;

public class ConsoleHintWithAutoLayout : ConsoleHint
{
	[SerializeField]
	private HintsLabelPlacement m_LabelPlacement;

	[SerializeField]
	private RectTransform m_BackgroundLeft;

	[SerializeField]
	private RectTransform m_BackgroundRight;

	[SerializeField]
	private bool m_ShowBackground = true;

	private RectTransform m_IconTransform;

	private HorizontalLayoutGroup m_HorizontalLayoutGroup;

	public HintsLabelPlacement LabelPlacement
	{
		get
		{
			return m_LabelPlacement;
		}
		set
		{
			m_LabelPlacement = value;
			SetLabelByType();
		}
	}

	private RectTransform IconTransform
	{
		get
		{
			if (!(m_IconTransform != null))
			{
				return m_IconTransform = m_Icon.GetComponent<RectTransform>();
			}
			return m_IconTransform;
		}
	}

	private HorizontalLayoutGroup HorizontalLayoutGroup
	{
		get
		{
			if (!(m_HorizontalLayoutGroup != null))
			{
				return m_HorizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
			}
			return m_HorizontalLayoutGroup;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		SetLabelByType();
	}

	private void SetLabelByType()
	{
		m_Label.gameObject.SetActive(m_LabelPlacement != HintsLabelPlacement.Empty);
		if (m_BackgroundLeft != null)
		{
			m_BackgroundLeft.gameObject.SetActive(m_LabelPlacement == HintsLabelPlacement.Left && m_ShowBackground);
		}
		if (m_BackgroundRight != null)
		{
			m_BackgroundRight.gameObject.SetActive(m_LabelPlacement == HintsLabelPlacement.Right && m_ShowBackground);
		}
		if (!(HorizontalLayoutGroup == null))
		{
			RectOffset padding = HorizontalLayoutGroup.padding;
			float x = 0.5f;
			TextAnchor childAlignment;
			switch (m_LabelPlacement)
			{
			case HintsLabelPlacement.Empty:
				padding.left = 0;
				padding.right = 0;
				childAlignment = TextAnchor.MiddleCenter;
				break;
			case HintsLabelPlacement.Left:
				padding.left = 5;
				padding.right = 20;
				x = 1f;
				childAlignment = TextAnchor.MiddleRight;
				break;
			case HintsLabelPlacement.Right:
				padding.left = 20;
				padding.right = 5;
				x = 0f;
				childAlignment = TextAnchor.MiddleLeft;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			HorizontalLayoutGroup.padding = padding;
			HorizontalLayoutGroup.childAlignment = childAlignment;
			RectTransform iconTransform = IconTransform;
			RectTransform iconTransform2 = IconTransform;
			Vector2 vector2 = (IconTransform.pivot = new Vector2(x, 0.5f));
			Vector2 anchorMax = (iconTransform2.anchorMin = vector2);
			iconTransform.anchorMax = anchorMax;
			IconTransform.pivot = new Vector2(0.5f, 0.5f);
			IconTransform.anchoredPosition = Vector2.zero;
		}
	}
}
