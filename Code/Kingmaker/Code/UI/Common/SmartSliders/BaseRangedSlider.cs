using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.Common.SmartSliders;

public abstract class BaseRangedSlider : MonoBehaviour
{
	[SerializeField]
	protected Slider m_MainSlider;

	[SerializeField]
	protected Image m_RangedImage;

	private RectTransform m_RangedImageTransform;

	private bool m_Horizontal;

	private bool m_Inverted;

	protected Tweener m_Tweener;

	public virtual void Initialize()
	{
		Slider.Direction direction = m_MainSlider.direction;
		m_Horizontal = direction == Slider.Direction.LeftToRight || direction == Slider.Direction.RightToLeft;
		m_Inverted = direction == Slider.Direction.RightToLeft || direction == Slider.Direction.TopToBottom;
		m_RangedImageTransform = (RectTransform)m_RangedImage.transform;
		SetNormalizedRange(0f, 0f);
	}

	public void SetMaxValue(float maxValue)
	{
		m_MainSlider.maxValue = maxValue;
	}

	protected void SetNormalizedRange(float from, float to)
	{
		from = Mathf.Clamp01(from);
		to = Mathf.Clamp01(to);
		if (m_Inverted)
		{
			from = 1f - from;
			to = 1f - to;
		}
		float num = Mathf.Min(from, to);
		float num2 = Mathf.Max(from, to);
		m_RangedImageTransform.anchorMin = new Vector2(m_Horizontal ? num : 0f, m_Horizontal ? 0f : num);
		m_RangedImageTransform.anchorMax = new Vector2(m_Horizontal ? num2 : 1f, m_Horizontal ? 1f : num2);
		m_RangedImageTransform.anchoredPosition = Vector2.zero;
		m_RangedImageTransform.sizeDelta = Vector2.zero;
	}

	protected virtual void OnDestroy()
	{
		m_Tweener?.Kill();
	}
}
