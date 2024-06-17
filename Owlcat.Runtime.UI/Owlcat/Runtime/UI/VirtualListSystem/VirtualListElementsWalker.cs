using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.UI.VirtualListSystem.Grid;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem;

internal class VirtualListElementsWalker
{
	private VirtualListLayoutEngineContext m_EngineContext;

	private IVirtualListLayoutEngine m_LayoutEngine;

	private VirtualListViewsFabric m_Fabric;

	private RectTransform m_Viewport;

	private RectTransform m_Content;

	private List<VirtualListElement> m_Elements;

	private bool m_LayoutNeedUpdate;

	private List<VirtualListElement> m_VisibleElements;

	private List<VirtualListElement> m_NewVisibleElements;

	private readonly List<VirtualListElement> m_Buffer = new List<VirtualListElement>();

	private VirtualListDistancesCalculator m_DistancesCalculator;

	private bool m_IsGrid;

	private bool m_ClearItemsAnyway;

	internal VirtualListElementsWalker(List<VirtualListElement> elements, List<VirtualListElement> visibleElements, RectTransform viewport, RectTransform content, IVirtualListLayoutSettings layoutSettings, VirtualListViewsFabric fabric, VirtualListScroll virtualListScroll, VirtualListDistancesCalculator distancesCalculator, bool clearItemsAnyway)
	{
		m_Fabric = fabric;
		m_Viewport = viewport;
		m_Content = content;
		m_Elements = elements;
		m_VisibleElements = visibleElements;
		m_DistancesCalculator = distancesCalculator;
		m_ClearItemsAnyway = clearItemsAnyway;
		m_EngineContext = new VirtualListLayoutEngineContext();
		m_LayoutEngine = LayoutEngineFabric.CreateLayoutEngine(layoutSettings, m_EngineContext, m_DistancesCalculator, viewport, content);
		m_NewVisibleElements = new List<VirtualListElement>();
		virtualListScroll.ScrollValueUpdated += SetDirtyWithScroll;
		m_IsGrid = m_LayoutEngine is VirtualListLayoutEngineGrid;
	}

	internal void SetDirty(VirtualListElement element)
	{
		if (m_LayoutNeedUpdate || !m_Elements.Contains(element))
		{
			return;
		}
		if (m_IsGrid)
		{
			m_LayoutNeedUpdate = true;
			return;
		}
		if (m_VisibleElements.Count == 0 || m_VisibleElements.Contains(element))
		{
			m_LayoutNeedUpdate = true;
			return;
		}
		int num = m_Elements.IndexOf(element);
		int num2 = num - 1;
		if (num2 >= 0 && m_VisibleElements.Contains(m_Elements[num2]))
		{
			m_LayoutNeedUpdate = true;
			return;
		}
		int num3 = num + 1;
		if (num3 < m_Elements.Count && m_VisibleElements.Contains(m_Elements[num3]))
		{
			m_LayoutNeedUpdate = true;
		}
	}

	private void SetDirtyWithScroll()
	{
		m_LayoutNeedUpdate = true;
	}

	internal bool Tick(bool force = false)
	{
		if (m_LayoutNeedUpdate || m_DistancesCalculator.ViewportSizeUpdated || m_DistancesCalculator.HasPinnedElement || force)
		{
			try
			{
				UpdateElements();
			}
			finally
			{
				m_LayoutNeedUpdate = false;
			}
			return true;
		}
		return false;
	}

	internal void Clear()
	{
		m_NewVisibleElements.Clear();
		ReturnInvisibleElements();
	}

	private void UpdateElements()
	{
		if (!m_DistancesCalculator.ViewportIsZeroSize)
		{
			m_Buffer.Clear();
			m_Buffer.AddRange(m_VisibleElements);
			if (m_DistancesCalculator.HasPinnedElement && m_DistancesCalculator.PinnedElementIndex >= 0 && m_DistancesCalculator.PinnedElementIndex < m_Elements.Count)
			{
				UpdateFromPinnedElement();
			}
			else if (!m_DistancesCalculator.HasVisibleElements && m_DistancesCalculator.ClosestIndex < 0)
			{
				UpdateFromClear();
			}
			else
			{
				UpdateUsually();
			}
			m_Buffer.Clear();
			ReturnInvisibleElements();
		}
	}

	private void UpdateFromPinnedElement()
	{
		m_EngineContext.CurrentElementIndex = m_DistancesCalculator.PinnedElementIndex;
		if (m_DistancesCalculator.PinnedElementIsOnTop)
		{
			m_EngineContext.UpdateType = VirtualListUpdateType.FromTopToBottom;
			m_LayoutEngine.SetOffset(m_DistancesCalculator.PinnedElementBorder);
			ProcessElementAt(m_DistancesCalculator.PinnedElementIndex);
			UpdateFromTop(m_DistancesCalculator.PinnedElementIndex, out var stopIndex);
			UpdateFromBottom(stopIndex);
		}
		else
		{
			m_EngineContext.UpdateType = VirtualListUpdateType.FromBottomToTop;
			m_LayoutEngine.SetOffset(m_DistancesCalculator.PinnedElementBorder);
			ProcessElementAt(m_DistancesCalculator.PinnedElementIndex);
			UpdateFromBottom(m_DistancesCalculator.PinnedElementIndex, out var stopIndex2);
			UpdateFromTop(stopIndex2);
		}
	}

	private void UpdateFromClear()
	{
		m_EngineContext.UpdateType = VirtualListUpdateType.FromTopToBottom;
		m_LayoutEngine.SetClear();
		ProcessElementAt(0);
		UpdateFromTop(0);
	}

	private void UpdateUsually()
	{
		int num = (m_DistancesCalculator.HasVisibleElements ? m_DistancesCalculator.TopVisibleIndex : m_DistancesCalculator.ClosestIndex);
		int stopIndex;
		if (num == 0)
		{
			m_EngineContext.UpdateType = VirtualListUpdateType.FromBottomToTop;
			m_EngineContext.CurrentElementIndex = 0;
			m_LayoutEngine.SetOffsetElement(m_Elements[0], forItself: true);
			m_EngineContext.UpdateType = VirtualListUpdateType.FromTopToBottom;
			ProcessElementAt(0);
			stopIndex = 0;
		}
		else
		{
			UpdateFromBottom(num, out stopIndex);
		}
		UpdateFromTop(stopIndex);
	}

	private void UpdateFromTop(int index)
	{
		UpdateFromTop(index, out var _);
	}

	private void UpdateFromTop(int index, out int stopIndex)
	{
		bool flag = m_LayoutEngine.IsInFieldOfView(m_Elements[index]);
		m_EngineContext.UpdateType = VirtualListUpdateType.FromTopToBottom;
		m_EngineContext.CurrentElementIndex = index;
		m_LayoutEngine.SetOffsetElement(m_Elements[index]);
		for (int i = index + 1; i < m_Elements.Count; i++)
		{
			bool flag2 = ProcessElementAt(i);
			m_LayoutEngine.SetOffsetElement(m_Elements[i]);
			if (!flag && flag2)
			{
				flag = true;
			}
			else if (flag && !flag2)
			{
				stopIndex = i;
				return;
			}
		}
		stopIndex = m_Elements.Count - 1;
	}

	private void UpdateFromBottom(int index)
	{
		UpdateFromBottom(index, out var _);
	}

	private void UpdateFromBottom(int index, out int stopIndex)
	{
		bool flag = m_LayoutEngine.IsInFieldOfView(m_Elements[index]);
		m_EngineContext.UpdateType = VirtualListUpdateType.FromBottomToTop;
		m_EngineContext.CurrentElementIndex = index;
		m_LayoutEngine.SetOffsetElement(m_Elements[index]);
		for (int num = index - 1; num >= 0; num--)
		{
			bool flag2 = ProcessElementAt(num);
			m_LayoutEngine.SetOffsetElement(m_Elements[num]);
			if (!flag && flag2)
			{
				flag = true;
			}
			else if (flag && !flag2)
			{
				stopIndex = num;
				return;
			}
		}
		stopIndex = 0;
	}

	private void ReturnInvisibleElements()
	{
		foreach (VirtualListElement item in m_VisibleElements.ToList())
		{
			if (!m_NewVisibleElements.Contains(item) && item.HasView())
			{
				m_Fabric.DetachView(item, m_ClearItemsAnyway);
			}
		}
		m_VisibleElements.Clear();
		m_VisibleElements.AddRange(m_NewVisibleElements);
		m_NewVisibleElements.Clear();
	}

	private bool ProcessElementAt(int index)
	{
		VirtualListElement virtualListElement = m_Elements[index];
		m_EngineContext.CurrentElementIndex = index;
		m_LayoutEngine.UpdatePosition(virtualListElement);
		if (!m_LayoutEngine.IsInFieldOfView(virtualListElement))
		{
			return false;
		}
		if (!m_NewVisibleElements.Contains(virtualListElement))
		{
			m_NewVisibleElements.Add(virtualListElement);
		}
		m_Buffer.Remove(virtualListElement);
		if (!virtualListElement.HasView())
		{
			VirtualListElement virtualListElement2 = null;
			if (m_Buffer.Count > 0)
			{
				int num = m_Buffer.Count - 1;
				int index2 = m_Fabric.GetIndex(virtualListElement);
				for (int num2 = num; num2 >= 0; num2--)
				{
					VirtualListElement virtualListElement3 = m_Buffer[num2];
					if (m_Fabric.GetIndex(virtualListElement3) == index2)
					{
						virtualListElement2 = virtualListElement3;
						m_Buffer.RemoveAt(num2);
						break;
					}
				}
			}
			if (virtualListElement2 != null)
			{
				IVirtualListElementView view = virtualListElement2.View;
				virtualListElement2.DetachView();
				virtualListElement.AttachView(view);
			}
			else
			{
				m_Fabric.AttachView(virtualListElement, m_Content);
			}
		}
		if (virtualListElement.HasLayoutSettings())
		{
			if (virtualListElement.IsControlledByUnityLayout())
			{
				virtualListElement.UpdateViewRectTransform();
				virtualListElement.RebuildUnitLayout();
			}
			m_LayoutEngine.UpdatePosition(virtualListElement);
		}
		virtualListElement.UpdateViewRectTransform();
		return true;
	}
}
