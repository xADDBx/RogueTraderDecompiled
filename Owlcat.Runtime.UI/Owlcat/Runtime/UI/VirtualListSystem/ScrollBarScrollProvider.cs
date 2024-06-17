using UnityEngine.UI;

namespace Owlcat.Runtime.UI.VirtualListSystem;

internal class ScrollBarScrollProvider : IScrollProvider
{
	private Scrollbar m_Scrollbar;

	private DragTracker m_ScrollbarDragTracker;

	private VirtualListDistancesCalculator m_DistancesCalculator;

	private bool m_IsVertical;

	private bool m_ScrollValueUpdated;

	private bool m_UpdatingScrollValue;

	internal ScrollBarScrollProvider(Scrollbar scrollbar, VirtualListDistancesCalculator distancesCalculator, bool isVertical)
	{
		m_Scrollbar = scrollbar;
		m_DistancesCalculator = distancesCalculator;
		m_IsVertical = isVertical;
		if (isVertical)
		{
			m_Scrollbar.direction = Scrollbar.Direction.TopToBottom;
		}
		else
		{
			m_Scrollbar.direction = Scrollbar.Direction.LeftToRight;
		}
		m_Scrollbar.onValueChanged.AddListener(OnScrollValueUpdated);
		m_ScrollbarDragTracker = m_Scrollbar.GetComponent<DragTracker>();
		if (m_ScrollbarDragTracker == null)
		{
			m_ScrollbarDragTracker = m_Scrollbar.gameObject.AddComponent<DragTracker>();
		}
	}

	private void OnScrollValueUpdated(float value)
	{
		if (!m_UpdatingScrollValue)
		{
			m_ScrollValueUpdated = true;
		}
	}

	public bool ScrollUpdated()
	{
		if (!m_ScrollValueUpdated)
		{
			return m_ScrollbarDragTracker.IsDragged;
		}
		return true;
	}

	public float GetScrollValue()
	{
		return m_Scrollbar.value;
	}

	public void SetScrollValue(float value)
	{
		m_UpdatingScrollValue = true;
		m_Scrollbar.value = value;
		m_UpdatingScrollValue = false;
		m_ScrollValueUpdated = false;
		float viewportSize = m_DistancesCalculator.ViewportSize;
		float contentPredictedSize = m_DistancesCalculator.ContentPredictedSize;
		if (viewportSize > contentPredictedSize)
		{
			m_Scrollbar.gameObject.SetActive(value: false);
			return;
		}
		m_Scrollbar.gameObject.SetActive(value: true);
		if (contentPredictedSize > 1f)
		{
			m_Scrollbar.size = viewportSize / contentPredictedSize;
		}
	}
}
