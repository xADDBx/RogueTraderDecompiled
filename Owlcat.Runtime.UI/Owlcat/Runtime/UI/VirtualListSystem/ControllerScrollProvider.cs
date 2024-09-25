using System;
using System.Collections.Generic;
using System.Linq;

namespace Owlcat.Runtime.UI.VirtualListSystem;

internal class ControllerScrollProvider : IScrollProvider, IInternalScrollController, IScrollController
{
	private IReadOnlyList<VirtualListElement> m_Elements;

	private VirtualListDistancesCalculator m_DistancesCalculator;

	private bool m_ScrollValueUpdated;

	private bool m_ForceScroll;

	private float m_LastValue;

	private float m_ScrollDelta;

	internal ControllerScrollProvider(IReadOnlyList<VirtualListElement> elements, VirtualListDistancesCalculator distancesCalculator)
	{
		m_Elements = elements;
		m_DistancesCalculator = distancesCalculator;
	}

	public bool ScrollUpdated()
	{
		return m_ScrollValueUpdated;
	}

	public float GetScrollValue()
	{
		if (m_ForceScroll)
		{
			return m_ScrollDelta;
		}
		return m_LastValue + m_ScrollDelta;
	}

	public void SetScrollValue(float value)
	{
		m_LastValue = value;
		m_ScrollValueUpdated = false;
		m_ForceScroll = false;
	}

	public void Scroll(float speed)
	{
		SetScrollDelta(speed);
	}

	public bool ElementIsInScrollZone(VirtualListElement element, out bool needScrollDown)
	{
		return !m_DistancesCalculator.ElementIsOutOfScrollZone(element, out needScrollDown);
	}

	public void ScrollTowards(IVirtualListElementData data, float speed)
	{
		VirtualListElement element = m_Elements.FirstOrDefault((VirtualListElement e) => e.Data == data);
		ScrollTowards(element, speed);
	}

	public void ScrollTowards(VirtualListElement element, float speed)
	{
		if (element != null && m_DistancesCalculator.ElementIsOutOfScrollZone(element, out var scrollDown))
		{
			if (scrollDown)
			{
				SetScrollDelta(Math.Abs(speed));
			}
			else
			{
				SetScrollDelta(0f - Math.Abs(speed));
			}
		}
	}

	public void ForceScrollToElement(IVirtualListElementData data)
	{
		VirtualListElement element = m_Elements.FirstOrDefault((VirtualListElement e) => e.Data == data);
		ForceScrollToElement(element);
	}

	public void ForceScrollToElement(VirtualListElement element)
	{
		if (element != null)
		{
			m_DistancesCalculator.TryPinToElement(element);
		}
	}

	public void ForceScrollToTop()
	{
		if (m_Elements.Count != 0)
		{
			m_DistancesCalculator.TryPinToElement(m_Elements[0]);
		}
	}

	public void ForceScrollToBottom()
	{
		if (m_Elements.Count != 0)
		{
			m_DistancesCalculator.TryPinToElement(m_Elements[m_Elements.Count - 1]);
		}
	}

	private void SetForceScrollDelta(float delta)
	{
		m_ScrollDelta = delta;
		m_ScrollValueUpdated = true;
		m_ForceScroll = true;
	}

	private void SetScrollDelta(float speed)
	{
		float num = m_DistancesCalculator.DistanceToTopOfContentFromTopOfViewport + m_DistancesCalculator.DistanceToBottomOfContentFromBottomOfViewport;
		m_ScrollDelta = speed / num;
		m_ScrollValueUpdated = true;
	}
}
