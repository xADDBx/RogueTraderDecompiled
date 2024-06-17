using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Owlcat.Runtime.UI.VirtualListSystem;

internal class VirtualListScroll
{
	private List<VirtualListElement> m_Elements;

	private RectTransform m_Content;

	private IVirtualListLayoutSettings m_Settings;

	private VirtualListDistancesCalculator m_DistancesCalculator;

	private float m_ContentPosition;

	private float m_PreviousScrollValue;

	private float m_NewScrollValue;

	private bool m_RemovedVisibleElementInFrame;

	private bool m_ScrollValueWasUpdatedInFrame;

	private List<IScrollProvider> m_ScrollProviders;

	public readonly IInternalScrollController ScrollController;

	internal event Action ScrollValueUpdated;

	internal VirtualListScroll(List<VirtualListElement> elements, VirtualListScrollSettings scrollSettings, RectTransform viewPort, RectTransform content, IVirtualListLayoutSettings settings, Scrollbar scrollbar, VirtualListDistancesCalculator distancesCalculator)
	{
		m_Settings = settings;
		m_Content = content;
		m_DistancesCalculator = distancesCalculator;
		m_Elements = elements;
		m_ScrollProviders = new List<IScrollProvider>();
		if (scrollbar != null)
		{
			m_ScrollProviders.Add(new ScrollBarScrollProvider(scrollbar, distancesCalculator, settings.IsVertical));
		}
		if (scrollSettings.UseScrollWheel)
		{
			m_ScrollProviders.Add(new MouseWheelScrollProvider(viewPort.gameObject, distancesCalculator, scrollSettings));
		}
		ControllerScrollProvider controllerScrollProvider = new ControllerScrollProvider(elements, distancesCalculator);
		m_ScrollProviders.Add(controllerScrollProvider);
		ScrollController = controllerScrollProvider;
		foreach (IScrollProvider scrollProvider in m_ScrollProviders)
		{
			scrollProvider.SetScrollValue(0f);
		}
		SetContentPosition(0f);
	}

	internal void OnElementRemoved(VirtualListElement element)
	{
		int num = m_Elements.IndexOf(element);
		int topVisibleIndex = m_DistancesCalculator.TopVisibleIndex;
		int bottomVisibleIndex = m_DistancesCalculator.BottomVisibleIndex;
		if (num >= topVisibleIndex && num <= bottomVisibleIndex + 1)
		{
			if (num == 0)
			{
				SetScrollValue(0f);
			}
			else if (bottomVisibleIndex == m_Elements.Count - 1)
			{
				SetScrollValue(1f);
			}
			m_RemovedVisibleElementInFrame = true;
		}
	}

	internal bool Tick()
	{
		if (m_DistancesCalculator.ContentPredictedSize > m_DistancesCalculator.ViewportSize)
		{
			TickScrollValue();
		}
		if (m_ScrollValueWasUpdatedInFrame || m_RemovedVisibleElementInFrame || m_DistancesCalculator.HasPinnedElement)
		{
			TickMoveContent();
			return true;
		}
		return false;
	}

	private void TickScrollValue()
	{
		m_ScrollValueWasUpdatedInFrame = false;
		foreach (IScrollProvider scrollProvider in m_ScrollProviders)
		{
			if (scrollProvider.ScrollUpdated())
			{
				m_NewScrollValue = scrollProvider.GetScrollValue();
				m_ScrollValueWasUpdatedInFrame = true;
				this.ScrollValueUpdated?.Invoke();
				break;
			}
		}
		if (!m_ScrollValueWasUpdatedInFrame)
		{
			return;
		}
		foreach (IScrollProvider scrollProvider2 in m_ScrollProviders)
		{
			scrollProvider2.SetScrollValue(m_NewScrollValue);
		}
	}

	private void TickMoveContent()
	{
		float num = m_NewScrollValue - m_PreviousScrollValue;
		if (m_DistancesCalculator.HasPinnedElement)
		{
			PinToPinnedElement();
		}
		else if (!m_DistancesCalculator.HasVisibleElements)
		{
			PinToClosest();
		}
		else if (m_NewScrollValue < 0.0001f)
		{
			PinToTop();
		}
		else if (1f - m_NewScrollValue < 0.0001f)
		{
			PinToBottom();
		}
		else if (num > 0f)
		{
			GoDown(num);
		}
		else
		{
			GoUp(num);
		}
		m_PreviousScrollValue = m_NewScrollValue;
		m_RemovedVisibleElementInFrame = false;
	}

	internal bool UpdateScrollValue()
	{
		if (m_ScrollValueWasUpdatedInFrame)
		{
			return false;
		}
		if (m_DistancesCalculator.DistanceToTopOfContentFromTopOfViewport < 0f || m_DistancesCalculator.ContentPredictedSize < m_DistancesCalculator.ViewportSize)
		{
			PinToTop();
			SetScrollValue(0f);
			return true;
		}
		if (m_DistancesCalculator.DistanceToBottomOfContentFromBottomOfViewport < 0f)
		{
			PinToBottom();
			SetScrollValue(1f);
			return true;
		}
		float num = m_DistancesCalculator.DistanceToTopOfContentFromTopOfViewport + m_DistancesCalculator.DistanceToBottomOfContentFromBottomOfViewport;
		float scrollValue = 0f;
		if ((double)num > 0.001)
		{
			scrollValue = m_DistancesCalculator.DistanceToTopOfContentFromTopOfViewport / num;
		}
		SetScrollValue(scrollValue);
		return true;
	}

	private void PinToTop()
	{
		AddToContentPosition(0f - m_DistancesCalculator.DistanceToTopOfContentFromTopOfViewport);
		SetContentPosition(m_ContentPosition);
	}

	private void PinToBottom()
	{
		AddToContentPosition(m_DistancesCalculator.DistanceToBottomOfContentFromBottomOfViewport);
		SetContentPosition(m_ContentPosition);
	}

	private void PinToClosest()
	{
		if (m_DistancesCalculator.ClosestIndex < 0)
		{
			m_ContentPosition = 0f;
			SetContentPosition(m_ContentPosition);
		}
		else
		{
			AddToContentPosition(m_DistancesCalculator.DistanceToClosest);
			SetContentPosition(m_ContentPosition);
		}
	}

	private void PinToPinnedElement()
	{
		if (m_DistancesCalculator.RefreshScrollWhenPinToElement)
		{
			m_ContentPosition = 0f;
			AddToContentPosition(m_DistancesCalculator.NewScrollPositionWhenPinToElement);
			SetContentPosition(m_ContentPosition);
		}
		else
		{
			AddToContentPosition(m_DistancesCalculator.DistanceToPinnedElement);
			SetContentPosition(m_ContentPosition);
		}
	}

	private void GoDown(float delta)
	{
		float num = 0f;
		if (m_DistancesCalculator.DistanceToBottomOfContentFromBottomOfViewport > 0f && m_PreviousScrollValue < 1f)
		{
			num = m_DistancesCalculator.DistanceToBottomOfContentFromBottomOfViewport / (1f - m_PreviousScrollValue);
		}
		AddToContentPosition(delta * num);
		SetContentPosition(m_ContentPosition);
	}

	private void GoUp(float delta)
	{
		float num = 0f;
		if (m_DistancesCalculator.DistanceToTopOfContentFromTopOfViewport > 0f && m_PreviousScrollValue > 0f)
		{
			num = m_DistancesCalculator.DistanceToTopOfContentFromTopOfViewport / m_PreviousScrollValue;
		}
		AddToContentPosition(delta * num);
		SetContentPosition(m_ContentPosition);
	}

	private void AddToContentPosition(float delta)
	{
		if (m_Settings.IsVertical)
		{
			m_ContentPosition += delta;
		}
		else
		{
			m_ContentPosition -= delta;
		}
	}

	private void SetContentPosition(float position)
	{
		Vector2 vector = new Vector2(0f, 1f);
		m_Content.anchorMin = vector;
		m_Content.anchorMax = vector;
		Vector2 vector2FromFloat = m_Settings.GetVector2FromFloat(position);
		m_Content.offsetMin = vector2FromFloat;
		m_Content.offsetMax = vector2FromFloat;
	}

	public void Clear()
	{
		SetScrollValue(0f);
		m_ContentPosition = 0f;
		SetContentPosition(0f);
	}

	private void SetScrollValue(float value)
	{
		foreach (IScrollProvider scrollProvider in m_ScrollProviders)
		{
			scrollProvider.SetScrollValue(value);
		}
		m_NewScrollValue = value;
		m_PreviousScrollValue = value;
	}
}
