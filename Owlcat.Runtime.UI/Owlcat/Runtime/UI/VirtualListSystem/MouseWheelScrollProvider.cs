using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem;

internal class MouseWheelScrollProvider : IScrollProvider
{
	private VirtualListDistancesCalculator m_DistancesCalculator;

	private ScrollHandler m_ScrollHandler;

	private VirtualListScrollSettings m_ScrollSettings;

	private float m_LastValue;

	internal MouseWheelScrollProvider(GameObject viewPort, VirtualListDistancesCalculator distancesCalculator, VirtualListScrollSettings scrollSettings)
	{
		m_DistancesCalculator = distancesCalculator;
		m_ScrollHandler = viewPort.GetComponent<ScrollHandler>();
		if (m_ScrollHandler == null)
		{
			m_ScrollHandler = viewPort.AddComponent<ScrollHandler>();
		}
		m_ScrollSettings = scrollSettings;
	}

	public bool ScrollUpdated()
	{
		bool isScrolling = m_ScrollHandler.IsScrolling;
		m_ScrollHandler.Refresh();
		return isScrolling;
	}

	public float GetScrollValue()
	{
		float num = m_DistancesCalculator.DistanceToTopOfContentFromTopOfViewport + m_DistancesCalculator.DistanceToBottomOfContentFromBottomOfViewport;
		float num2 = m_ScrollHandler.ScrollDelta.y * m_ScrollSettings.ScrollWheelSpeed / num;
		return m_LastValue - num2;
	}

	public void SetScrollValue(float value)
	{
		m_LastValue = value;
	}
}
