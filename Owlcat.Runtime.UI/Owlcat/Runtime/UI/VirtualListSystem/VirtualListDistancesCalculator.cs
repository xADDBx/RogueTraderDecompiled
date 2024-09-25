using System;
using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem;

internal class VirtualListDistancesCalculator
{
	private List<VirtualListElement> m_Elements;

	private List<VirtualListElement> m_VisibleElements;

	private RectTransform m_Viewport;

	private RectTransform m_Content;

	private Vector2 m_DefaultSize;

	private IVirtualListLayoutSettings m_LayoutSettings;

	private VirtualListScrollSettings m_ScrollSettings;

	private bool m_NeedUpdate;

	internal float DistanceToTopOfContentFromTopOfViewport { get; private set; }

	internal float DistanceToBottomOfContentFromBottomOfViewport { get; private set; }

	internal bool HasVisibleElements { get; private set; }

	internal int TopVisibleIndex { get; private set; }

	internal int BottomVisibleIndex { get; private set; }

	internal int ClosestIndex { get; private set; }

	internal float DistanceToClosest { get; private set; }

	public float ViewportTopBorder { get; private set; }

	public float ViewportBottomBorder { get; private set; }

	public float ViewportSize { get; private set; }

	public float ContentPredictedSize { get; private set; }

	public bool ViewportIsZeroSize { get; private set; }

	public bool ViewportSizeUpdated { get; private set; }

	internal bool HasPinnedElement { get; private set; }

	internal int PinnedElementIndex { get; private set; }

	internal float DistanceToPinnedElement { get; private set; }

	internal float PinnedElementBorder { get; private set; }

	internal bool PinnedElementIsOnTop { get; private set; }

	internal bool RefreshScrollWhenPinToElement { get; private set; }

	internal float NewScrollPositionWhenPinToElement { get; private set; }

	internal VirtualListDistancesCalculator(List<VirtualListElement> elements, List<VirtualListElement> visibleElements, RectTransform viewport, RectTransform content, IVirtualListLayoutSettings layoutSettings, VirtualListScrollSettings scrollSettings)
	{
		m_Elements = elements;
		m_VisibleElements = visibleElements;
		m_Viewport = viewport;
		m_Content = content;
		m_LayoutSettings = layoutSettings;
		m_ScrollSettings = scrollSettings;
		Clear();
	}

	internal bool Tick(bool force = false)
	{
		if (m_NeedUpdate || force)
		{
			try
			{
				UpdateValues();
			}
			finally
			{
				m_NeedUpdate = false;
			}
			return true;
		}
		return false;
	}

	internal void SetDirty()
	{
		m_NeedUpdate = true;
	}

	internal void OnElementRemoved(VirtualListElement element)
	{
		int num = m_Elements.IndexOf(element);
		if (num < TopVisibleIndex)
		{
			TopVisibleIndex--;
		}
		if (num < BottomVisibleIndex)
		{
			BottomVisibleIndex--;
		}
		m_NeedUpdate = true;
	}

	private void UpdateValues()
	{
		if (m_VisibleElements.Count > 0)
		{
			HasVisibleElements = true;
			TopVisibleIndex = GetTopVisibleIndex();
			BottomVisibleIndex = GetBottomVisibleIndex();
			DistanceToTopOfContentFromTopOfViewport = GetDistanceToTop();
			DistanceToBottomOfContentFromBottomOfViewport = GetDistanceToBottom();
			ContentPredictedSize = DistanceToTopOfContentFromTopOfViewport + DistanceToBottomOfContentFromBottomOfViewport + ViewportSize;
			ClosestIndex = -1;
			DistanceToClosest = 0f;
		}
		else
		{
			HasVisibleElements = false;
			TopVisibleIndex = -1;
			BottomVisibleIndex = -1;
			FindClosestElement();
			AddPaddingToClosestElementDistance();
		}
	}

	internal void UpdateViewportData()
	{
		float floatFromVector = m_LayoutSettings.GetFloatFromVector2(m_Viewport.rect.size);
		ViewportSizeUpdated = Mathf.Abs(floatFromVector - ViewportSize) > 0.01f;
		if (ViewportSizeUpdated)
		{
			ViewportSize = floatFromVector;
		}
		ViewportTopBorder = 0f - m_LayoutSettings.GetFloatFromVector2(m_Content.offsetMin);
		ViewportBottomBorder = ViewportTopBorder + (m_LayoutSettings.IsVertical ? (0f - ViewportSize) : ViewportSize);
		ViewportIsZeroSize = ViewportSize < 0.01f;
	}

	private float GetDistanceToTop()
	{
		if (TopVisibleIndex < 0)
		{
			return 0f;
		}
		return GetDistanceFromTopBorderOfViewportToTopBorderOfElement(m_Elements[TopVisibleIndex]) + GetPredictedDistanceFromElementToTop(TopVisibleIndex - 1);
	}

	private float GetDistanceToBottom()
	{
		if (BottomVisibleIndex < 0)
		{
			return 0f;
		}
		return GetDistanceFromBottomBorderOfViewportToBottomBorderOfElement(m_Elements[BottomVisibleIndex]) + GetPredictedDistanceFromElementToBottom(BottomVisibleIndex + 1);
	}

	private void FindClosestElement()
	{
		if (m_Elements.Count == 0 || !m_Elements[0].WasUpdatedAtLeastOnes)
		{
			ClosestIndex = -1;
			DistanceToClosest = 0f;
			return;
		}
		if (!IsElementOnTopOfContent(m_Elements[0], isTopBorder: true))
		{
			ClosestIndex = 0;
			DistanceToClosest = 0f - GetDistanceFromTopBorderOfViewportToTopBorderOfElement(m_Elements[0]);
			return;
		}
		int num = 0;
		bool flag = false;
		for (int i = 1; i < m_Elements.Count; i++)
		{
			num = i;
			if (!m_Elements[i].WasUpdatedAtLeastOnes)
			{
				break;
			}
			if (!IsElementOnTopOfContent(m_Elements[i], isTopBorder: true))
			{
				flag = true;
				break;
			}
		}
		float floatFromVector = m_LayoutSettings.GetFloatFromVector2(m_Viewport.rect.size);
		if (flag)
		{
			if (GetPredictedDistanceFromElementToTop(num) > floatFromVector)
			{
				ClosestIndex = num;
				DistanceToClosest = GetDistanceFromBottomBorderOfViewportToBottomBorderOfElement(m_Elements[num]);
				return;
			}
			while (num > 0 && GetPredictedDistanceFromElementToBottom(num) < floatFromVector)
			{
				num--;
			}
			ClosestIndex = num;
			if (IsElementOnTopOfContent(m_Elements[num], isTopBorder: true))
			{
				DistanceToClosest = 0f - GetDistanceFromTopBorderOfViewportToTopBorderOfElement(m_Elements[num]);
			}
			else
			{
				DistanceToClosest = GetDistanceFromTopBorderOfViewportToTopBorderOfElement(m_Elements[num]);
			}
		}
		else if (m_Elements[num].WasUpdatedAtLeastOnes && GetPredictedDistanceFromElementToTop(num) > floatFromVector)
		{
			ClosestIndex = num;
			DistanceToClosest = GetDistanceFromBottomBorderOfViewportToBottomBorderOfElement(m_Elements[num]);
		}
		else
		{
			while (num > 0 && (!m_Elements[num].WasUpdatedAtLeastOnes || GetPredictedDistanceFromElementToBottom(num) < floatFromVector))
			{
				num--;
			}
			ClosestIndex = num;
			DistanceToClosest = 0f - GetDistanceFromTopBorderOfViewportToTopBorderOfElement(m_Elements[num]);
		}
	}

	internal void TryPinToElement(VirtualListElement element)
	{
		PinnedElementIndex = m_Elements.IndexOf(element);
		if (element.WasUpdatedAtLeastOnes)
		{
			float distanceFromTopBorderOfViewportToTopBorderOfElement = GetDistanceFromTopBorderOfViewportToTopBorderOfElement(element);
			float distanceFromBottomBorderOfViewportToBottomBorderOfElement = GetDistanceFromBottomBorderOfViewportToBottomBorderOfElement(element);
			bool flag = distanceFromTopBorderOfViewportToTopBorderOfElement + m_ScrollSettings.ScrollZoneReduction > 0f;
			bool flag2 = distanceFromBottomBorderOfViewportToBottomBorderOfElement + m_ScrollSettings.ScrollZoneReduction > 0f;
			if ((flag && flag2) || (!flag && !flag2))
			{
				return;
			}
			if (flag)
			{
				if (PinnedElementIndex == 0 && distanceFromTopBorderOfViewportToTopBorderOfElement <= 0f)
				{
					return;
				}
				PinnedElementIsOnTop = true;
				PinnedElementBorder = (m_LayoutSettings.IsVertical ? m_LayoutSettings.GetFloatFromVector2(element.OffsetMax) : m_LayoutSettings.GetFloatFromVector2(element.OffsetMin));
				DistanceToPinnedElement = distanceFromTopBorderOfViewportToTopBorderOfElement + m_ScrollSettings.ScrollZoneReduction + 1f;
				if (DistanceToPinnedElement > DistanceToTopOfContentFromTopOfViewport)
				{
					DistanceToPinnedElement = DistanceToTopOfContentFromTopOfViewport;
				}
				DistanceToPinnedElement = 0f - DistanceToPinnedElement;
			}
			else
			{
				if (PinnedElementIndex == m_Elements.Count - 1 && distanceFromBottomBorderOfViewportToBottomBorderOfElement <= 0f)
				{
					return;
				}
				PinnedElementIsOnTop = false;
				PinnedElementBorder = (m_LayoutSettings.IsVertical ? m_LayoutSettings.GetFloatFromVector2(element.OffsetMin) : m_LayoutSettings.GetFloatFromVector2(element.OffsetMax));
				DistanceToPinnedElement = distanceFromBottomBorderOfViewportToBottomBorderOfElement + m_ScrollSettings.ScrollZoneReduction + 1f;
				if (DistanceToPinnedElement > DistanceToBottomOfContentFromBottomOfViewport)
				{
					DistanceToPinnedElement = DistanceToBottomOfContentFromBottomOfViewport;
				}
			}
		}
		else
		{
			if (PinnedElementIndex == 0)
			{
				return;
			}
			RefreshScrollWhenPinToElement = true;
			float predictedDistanceFromElementToTop = GetPredictedDistanceFromElementToTop(PinnedElementIndex - 1);
			float pinnedElementBorder = predictedDistanceFromElementToTop + m_LayoutSettings.DefaultSizeInScrollDirection;
			if (PinnedElementIndex < TopVisibleIndex)
			{
				PinnedElementIsOnTop = true;
				PinnedElementBorder = predictedDistanceFromElementToTop;
				Math.Min(m_ScrollSettings.ScrollZoneReduction, predictedDistanceFromElementToTop);
				NewScrollPositionWhenPinToElement = PinnedElementBorder - m_ScrollSettings.ScrollZoneReduction - 1f;
				if (NewScrollPositionWhenPinToElement < 0f)
				{
					NewScrollPositionWhenPinToElement = 0f;
				}
				if (m_LayoutSettings.IsVertical)
				{
					PinnedElementBorder = 0f - PinnedElementBorder;
				}
			}
			else
			{
				PinnedElementIsOnTop = false;
				PinnedElementBorder = pinnedElementBorder;
				float num = Math.Min(m_ScrollSettings.ScrollZoneReduction, GetPredictedDistanceFromElementToBottom(PinnedElementIndex + 1));
				NewScrollPositionWhenPinToElement = PinnedElementBorder + num + 1f - ViewportSize;
				if (NewScrollPositionWhenPinToElement < 0f)
				{
					NewScrollPositionWhenPinToElement = 0f;
				}
				if (m_LayoutSettings.IsVertical)
				{
					PinnedElementBorder = 0f - PinnedElementBorder;
				}
			}
		}
		m_NeedUpdate = false;
		HasPinnedElement = true;
	}

	private void AddPaddingToClosestElementDistance()
	{
		if (ClosestIndex == 0)
		{
			DistanceToClosest -= m_LayoutSettings.TopPaddingInScrollDirection;
		}
		if (ClosestIndex == m_Elements.Count - 1)
		{
			DistanceToClosest += m_LayoutSettings.BottomPaddingInScrollDirection;
		}
	}

	private float GetPredictedDistanceFromElementToTop(int elementIndex)
	{
		float num = 0f;
		for (int num2 = elementIndex; num2 >= 0; num2--)
		{
			if (m_LayoutSettings.IsEdgeIndex(elementIndex - num2))
			{
				num += m_LayoutSettings.DefaultSpacingIsScrollDirection;
				num += ((m_Elements[num2].HasLayoutSettings() && m_Elements[num2].WasUpdatedAtLeastOnes) ? m_LayoutSettings.GetFloatFromVector2(m_Elements[num2].Size) : m_LayoutSettings.DefaultSizeInScrollDirection);
			}
		}
		return num + m_LayoutSettings.TopPaddingInScrollDirection;
	}

	private float GetPredictedDistanceFromElementToBottom(int elementIndex)
	{
		float num = 0f;
		for (int i = elementIndex; i < m_Elements.Count; i++)
		{
			if (m_LayoutSettings.IsEdgeIndex(i - elementIndex))
			{
				num += m_LayoutSettings.DefaultSpacingIsScrollDirection;
				num += ((m_Elements[i].HasLayoutSettings() && m_Elements[i].WasUpdatedAtLeastOnes) ? m_LayoutSettings.GetFloatFromVector2(m_Elements[i].Size) : m_LayoutSettings.DefaultSizeInScrollDirection);
			}
		}
		return num + m_LayoutSettings.BottomPaddingInScrollDirection;
	}

	internal bool ElementIsOutOfScrollZone(VirtualListElement element, out bool scrollDown)
	{
		scrollDown = true;
		if (!element.WasUpdatedAtLeastOnes)
		{
			scrollDown = true;
			return true;
		}
		if (!element.HasView())
		{
			int num = m_Elements.IndexOf(element);
			scrollDown = num > TopVisibleIndex;
			return true;
		}
		float distanceFromTopBorderOfViewportToTopBorderOfElement = GetDistanceFromTopBorderOfViewportToTopBorderOfElement(element);
		float distanceFromBottomBorderOfViewportToBottomBorderOfElement = GetDistanceFromBottomBorderOfViewportToBottomBorderOfElement(element);
		bool flag = distanceFromTopBorderOfViewportToTopBorderOfElement + m_ScrollSettings.ScrollZoneReduction > 0f;
		bool flag2 = distanceFromBottomBorderOfViewportToBottomBorderOfElement + m_ScrollSettings.ScrollZoneReduction > 0f;
		if (flag && flag2)
		{
			return false;
		}
		if (flag)
		{
			if (DistanceToTopOfContentFromTopOfViewport <= 0f)
			{
				return false;
			}
			scrollDown = false;
			return true;
		}
		if (flag2)
		{
			if (DistanceToBottomOfContentFromBottomOfViewport < 0f)
			{
				return false;
			}
			scrollDown = true;
			return true;
		}
		return false;
	}

	private float GetDistanceFromTopBorderOfViewportToTopBorderOfElement(VirtualListElement element)
	{
		float floatFromVector = m_LayoutSettings.GetFloatFromVector2(m_Content.offsetMin);
		floatFromVector += (m_LayoutSettings.IsVertical ? m_LayoutSettings.GetFloatFromVector2(element.OffsetMax) : m_LayoutSettings.GetFloatFromVector2(element.OffsetMin));
		if (!m_LayoutSettings.IsVertical)
		{
			floatFromVector = 0f - floatFromVector;
		}
		return floatFromVector;
	}

	private float GetDistanceFromBottomBorderOfViewportToBottomBorderOfElement(VirtualListElement element)
	{
		float floatFromVector = m_LayoutSettings.GetFloatFromVector2(m_Content.offsetMin);
		floatFromVector += (m_LayoutSettings.IsVertical ? m_LayoutSettings.GetFloatFromVector2(element.OffsetMin) : m_LayoutSettings.GetFloatFromVector2(element.OffsetMax));
		if (m_LayoutSettings.IsVertical)
		{
			floatFromVector = 0f - floatFromVector;
		}
		return floatFromVector - ViewportSize;
	}

	private bool IsElementOnTopOfContent(VirtualListElement element, bool isTopBorder)
	{
		float floatFromVector = m_LayoutSettings.GetFloatFromVector2(m_Content.offsetMin);
		if (!isTopBorder)
		{
			if (!m_LayoutSettings.IsVertical)
			{
				return m_LayoutSettings.GetFloatFromVector2(element.OffsetMax) + floatFromVector < 0f;
			}
			return m_LayoutSettings.GetFloatFromVector2(element.OffsetMin) + floatFromVector > 0f;
		}
		if (!m_LayoutSettings.IsVertical)
		{
			return m_LayoutSettings.GetFloatFromVector2(element.OffsetMin) + floatFromVector < 0f;
		}
		return m_LayoutSettings.GetFloatFromVector2(element.OffsetMax) + floatFromVector > 0f;
	}

	private int GetTopVisibleIndex()
	{
		int num = m_Elements.Count + 1;
		foreach (VirtualListElement visibleElement in m_VisibleElements)
		{
			num = Math.Min(m_Elements.IndexOf(visibleElement), num);
		}
		if (num > m_Elements.Count)
		{
			num = -1;
		}
		return num;
	}

	private int GetBottomVisibleIndex()
	{
		int num = -1;
		foreach (VirtualListElement visibleElement in m_VisibleElements)
		{
			num = Math.Max(m_Elements.IndexOf(visibleElement), num);
		}
		return num;
	}

	public void Clear()
	{
		HasPinnedElement = false;
		RefreshScrollWhenPinToElement = false;
		HasVisibleElements = false;
		TopVisibleIndex = -1;
		BottomVisibleIndex = -1;
		DistanceToTopOfContentFromTopOfViewport = 0f;
		DistanceToBottomOfContentFromBottomOfViewport = 0f - ViewportSize;
		ClosestIndex = -1;
		DistanceToClosest = 0f;
	}

	public void ClearPinnedElement()
	{
		HasPinnedElement = false;
		RefreshScrollWhenPinToElement = false;
	}
}
