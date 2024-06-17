using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.Common;

[Serializable]
[RequireComponent(typeof(RectTransform))]
public class LayoutElementExtension : LayoutElement
{
	public float MaxHeight;

	public float MaxWidth;

	public bool UseMaxWidth;

	public bool UseMaxHeight;

	private bool m_IgnoreOnGettingPreferedSize;

	public override int layoutPriority
	{
		get
		{
			if (!m_IgnoreOnGettingPreferedSize)
			{
				return base.layoutPriority;
			}
			return -1;
		}
		set
		{
			base.layoutPriority = value;
		}
	}

	public override float preferredHeight
	{
		get
		{
			if (UseMaxHeight)
			{
				if (m_IgnoreOnGettingPreferedSize)
				{
					return base.preferredHeight;
				}
				bool ignoreOnGettingPreferedSize = m_IgnoreOnGettingPreferedSize;
				m_IgnoreOnGettingPreferedSize = true;
				float num = LayoutUtility.GetPreferredHeight(base.transform as RectTransform);
				m_IgnoreOnGettingPreferedSize = ignoreOnGettingPreferedSize;
				if (!(num > MaxHeight))
				{
					return num;
				}
				return MaxHeight;
			}
			return base.preferredHeight;
		}
		set
		{
			base.preferredHeight = value;
		}
	}

	public override float preferredWidth
	{
		get
		{
			if (UseMaxWidth)
			{
				if (m_IgnoreOnGettingPreferedSize)
				{
					return base.preferredWidth;
				}
				bool ignoreOnGettingPreferedSize = m_IgnoreOnGettingPreferedSize;
				m_IgnoreOnGettingPreferedSize = true;
				float num = LayoutUtility.GetPreferredWidth(base.transform as RectTransform);
				m_IgnoreOnGettingPreferedSize = ignoreOnGettingPreferedSize;
				if (!(num > MaxWidth))
				{
					return num;
				}
				return MaxWidth;
			}
			return base.preferredWidth;
		}
		set
		{
			base.preferredWidth = value;
		}
	}
}
