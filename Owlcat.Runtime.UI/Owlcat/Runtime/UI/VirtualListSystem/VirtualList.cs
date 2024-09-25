using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Owlcat.Runtime.UI.VirtualListSystem;

internal class VirtualList
{
	private VirtualListLayoutEngineContext m_EngineContext;

	private IVirtualListLayoutEngine m_LayoutEngine;

	private VirtualListViewsFabric m_Fabric;

	private List<VirtualListElement> m_Elements;

	private List<VirtualListElement> m_ActiveElements;

	private List<VirtualListElement> m_VisibleElements;

	private VirtualListElementsWalker m_ElementsWalker;

	private VirtualListScroll m_Scroll;

	private VirtualListDistancesCalculator m_DistancesCalculator;

	private bool m_NeedUpdateScrollValue;

	private VirtualListConsoleNavigation m_VirtualListConsoleNavigation;

	private bool m_IsValidView;

	public readonly ReactiveCommand AttachedFirstValidView = new ReactiveCommand();

	private bool m_ClearItemsAnyway;

	public List<VirtualListElement> Elements => m_Elements;

	public List<VirtualListElement> ActiveElements => m_ActiveElements;

	public List<VirtualListElement> VisibleElements => m_VisibleElements;

	internal IScrollController ScrollController => m_Scroll?.ScrollController;

	public VirtualList(IVirtualListLayoutSettings layoutSettings, VirtualListScrollSettings scrollSettings, VirtualListViewsFabric fabric, RectTransform viewport, RectTransform content, Scrollbar scrollbar, bool clearItemsAnyway)
	{
		m_Fabric = fabric;
		fabric.AttachedValidView += AttachedValidView;
		m_Elements = new List<VirtualListElement>();
		m_ActiveElements = new List<VirtualListElement>();
		m_VisibleElements = new List<VirtualListElement>();
		m_ClearItemsAnyway = clearItemsAnyway;
		m_DistancesCalculator = new VirtualListDistancesCalculator(m_ActiveElements, m_VisibleElements, viewport, content, layoutSettings, scrollSettings);
		m_Scroll = new VirtualListScroll(m_ActiveElements, scrollSettings, viewport, content, layoutSettings, scrollbar, m_DistancesCalculator);
		m_ElementsWalker = new VirtualListElementsWalker(m_ActiveElements, m_VisibleElements, viewport, content, layoutSettings, fabric, m_Scroll, m_DistancesCalculator, clearItemsAnyway);
		if (GamePad.Instance.Type != 0)
		{
			m_VirtualListConsoleNavigation = new VirtualListConsoleNavigation(layoutSettings, scrollSettings, m_Scroll.ScrollController);
		}
		m_DistancesCalculator.UpdateViewportData();
	}

	internal GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		return m_VirtualListConsoleNavigation?.GetNavigationBehaviour(m_Elements);
	}

	internal GridConsoleNavigationBehaviour GetActiveNavigationBehaviour()
	{
		return m_VirtualListConsoleNavigation?.GetNavigationBehaviour(m_ActiveElements);
	}

	internal void ClearNavigation()
	{
		m_VirtualListConsoleNavigation?.Clear();
		m_VirtualListConsoleNavigation?.ResetNavigation();
	}

	private void Add<TData>(TData data) where TData : IVirtualListElementData
	{
		VirtualListElement virtualListElement = CreateNewElement(data);
		m_Elements.Add(virtualListElement);
		m_VirtualListConsoleNavigation?.AddElement(virtualListElement);
		if (virtualListElement.IsActive)
		{
			AddToActiveElements(virtualListElement);
		}
	}

	internal void AddRange<TData>(IEnumerable<TData> dataRange) where TData : IVirtualListElementData
	{
		foreach (TData item in dataRange)
		{
			Add(item);
		}
	}

	internal void Insert<TData>(int index, TData data) where TData : IVirtualListElementData
	{
		VirtualListElement virtualListElement = CreateNewElement(data);
		m_Elements.Insert(index, virtualListElement);
		m_VirtualListConsoleNavigation?.InsertElement(index, virtualListElement);
		if (virtualListElement.IsActive)
		{
			InsertInActiveElements(index, virtualListElement);
		}
	}

	internal void Replace(IVirtualListElementData oldData, IVirtualListElementData newData)
	{
		int num = m_Elements.FindIndex((VirtualListElement e) => e.Data == oldData);
		if (num >= 0)
		{
			VirtualListElement virtualListElement = m_Elements[num];
			RemoveFromActiveElements(virtualListElement);
			virtualListElement.UnsubscribeFromActiveUpdate(SetElementActive);
			virtualListElement.UnsubscribeFromData();
			m_Elements.Remove(virtualListElement);
			m_VirtualListConsoleNavigation?.RemoveElement(virtualListElement);
			Insert(num, newData);
		}
	}

	internal void Remove(IVirtualListElementData data)
	{
		VirtualListElement virtualListElement = m_Elements.FirstOrDefault((VirtualListElement e) => e.Data == data);
		if (virtualListElement != null && m_Elements.Contains(virtualListElement))
		{
			RemoveFromActiveElements(virtualListElement);
			virtualListElement.UnsubscribeFromActiveUpdate(SetElementActive);
			virtualListElement.UnsubscribeFromData();
			m_Elements.Remove(virtualListElement);
			m_VirtualListConsoleNavigation?.RemoveElement(virtualListElement);
		}
	}

	internal void Move(int oldIndex, int newIndex)
	{
		if (oldIndex != newIndex)
		{
			VirtualListElement virtualListElement = m_Elements[oldIndex];
			RemoveFromActiveElements(virtualListElement);
			m_Elements.Remove(virtualListElement);
			m_VirtualListConsoleNavigation?.RemoveElement(virtualListElement);
			m_Elements.Insert(newIndex, virtualListElement);
			m_VirtualListConsoleNavigation?.InsertElement(newIndex, virtualListElement);
			if (virtualListElement.IsActive)
			{
				InsertInActiveElements(newIndex, virtualListElement);
			}
		}
	}

	internal void Clear()
	{
		m_ElementsWalker.Clear();
		m_DistancesCalculator.Clear();
		m_DistancesCalculator.UpdateViewportData();
		m_Scroll.Clear();
		m_VirtualListConsoleNavigation?.Clear();
		foreach (VirtualListElement item in m_Elements.ToList())
		{
			Remove(item.Data);
		}
		m_ActiveElements.Clear();
		m_VisibleElements.Clear();
		m_IsValidView = false;
	}

	internal void Dispose()
	{
		m_Fabric.AttachedValidView -= AttachedValidView;
		Clear();
	}

	private void SetElementActive(VirtualListElement element)
	{
		if (element.IsActive)
		{
			InsertInActiveElements(m_Elements.IndexOf(element), element);
		}
		else
		{
			RemoveFromActiveElements(element);
		}
	}

	private void AddToActiveElements(VirtualListElement element)
	{
		if (!m_ActiveElements.Contains(element))
		{
			element.SubscribeOnLayoutUpdate(m_ElementsWalker.SetDirty);
			m_ActiveElements.Add(element);
			m_DistancesCalculator.SetDirty();
			m_ElementsWalker.SetDirty(element);
		}
	}

	private void InsertInActiveElements(int index, VirtualListElement element)
	{
		element.SubscribeOnLayoutUpdate(m_ElementsWalker.SetDirty);
		if (index == 0)
		{
			m_ActiveElements.Insert(0, element);
		}
		else if (index >= m_Elements.Count - 1)
		{
			m_ActiveElements.Add(element);
		}
		else
		{
			int num = -1;
			for (int i = index + 1; i < m_Elements.Count; i++)
			{
				if (m_Elements[i].IsActive)
				{
					num = m_ActiveElements.IndexOf(m_Elements[i]);
					break;
				}
			}
			if (num >= 0)
			{
				m_ActiveElements.Insert(num, element);
			}
			else
			{
				m_ActiveElements.Add(element);
			}
		}
		m_DistancesCalculator.SetDirty();
		m_ElementsWalker.SetDirty(element);
	}

	private void RemoveFromActiveElements(VirtualListElement element)
	{
		if (m_ActiveElements.Contains(element))
		{
			m_Scroll.OnElementRemoved(element);
			m_DistancesCalculator.OnElementRemoved(element);
			m_ElementsWalker.SetDirty(element);
			if (element.HasView())
			{
				m_Fabric.DetachView(element, m_ClearItemsAnyway);
			}
			m_VisibleElements.Remove(element);
			element.UnsubscribeFromLayoutUpdate(m_ElementsWalker.SetDirty);
			m_ActiveElements.Remove(element);
		}
	}

	private VirtualListElement CreateNewElement(IVirtualListElementData data)
	{
		VirtualListElement virtualListElement = new VirtualListElement(data);
		virtualListElement.SubscribeOnData();
		virtualListElement.SubscribeOnActiveUpdate(SetElementActive);
		return virtualListElement;
	}

	private void AttachedValidView()
	{
		if (!m_IsValidView)
		{
			m_IsValidView = true;
			AttachedFirstValidView.Execute();
		}
	}

	public void Tick()
	{
		if (m_ActiveElements.Count != 0)
		{
			bool flag = m_DistancesCalculator.Tick();
			bool num = m_Scroll.Tick();
			if (num)
			{
				flag |= m_DistancesCalculator.Tick(force: true);
			}
			m_DistancesCalculator.UpdateViewportData();
			bool flag2 = m_ElementsWalker.Tick();
			if (num || flag2)
			{
				flag |= m_DistancesCalculator.Tick(force: true);
				m_VirtualListConsoleNavigation?.NavigationBehaviour?.DeepestNestedFocus?.SetFocused(value: true);
			}
			m_NeedUpdateScrollValue |= flag2 || flag || m_DistancesCalculator.ViewportSizeUpdated;
			if (m_NeedUpdateScrollValue && m_Scroll.UpdateScrollValue())
			{
				m_NeedUpdateScrollValue = false;
				m_DistancesCalculator.UpdateViewportData();
				m_ElementsWalker.Tick(force: true);
			}
			m_DistancesCalculator.ClearPinnedElement();
		}
	}

	public IConsoleEntity TryGetNavigationEntity(IViewModel viewModel)
	{
		return Elements.FirstOrDefault((VirtualListElement el) => el.Data is IViewModel viewModel2 && viewModel2 == viewModel);
	}
}
