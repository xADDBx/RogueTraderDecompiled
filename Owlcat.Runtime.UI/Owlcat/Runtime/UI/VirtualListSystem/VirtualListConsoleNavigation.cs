using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.VirtualListSystem.Grid;
using Owlcat.Runtime.UI.VirtualListSystem.Horizontal;
using Owlcat.Runtime.UI.VirtualListSystem.Vertical;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem;

internal class VirtualListConsoleNavigation : IConsoleNavigationScroll
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private IVirtualListLayoutSettings m_LayoutSettings;

	private VirtualListScrollSettings m_ScrollSettings;

	private IInternalScrollController m_Scroll;

	private IDisposable m_FocusSubscription;

	internal GridConsoleNavigationBehaviour NavigationBehaviour => m_NavigationBehaviour;

	internal VirtualListConsoleNavigation(IVirtualListLayoutSettings layoutSettings, VirtualListScrollSettings scrollSettings, IInternalScrollController scroll)
	{
		m_LayoutSettings = layoutSettings;
		m_ScrollSettings = scrollSettings;
		m_Scroll = scroll;
	}

	internal GridConsoleNavigationBehaviour GetNavigationBehaviour(List<VirtualListElement> elements)
	{
		if (m_NavigationBehaviour != null)
		{
			return m_NavigationBehaviour;
		}
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour(null, this);
		m_NavigationBehaviour.ContextName = "VList Nav";
		IVirtualListLayoutSettings layoutSettings = m_LayoutSettings;
		if (!(layoutSettings is VirtualListLayoutSettingsGrid virtualListLayoutSettingsGrid))
		{
			if (!(layoutSettings is VirtualListLayoutSettingsHorizontal))
			{
				if (layoutSettings is VirtualListLayoutSettingsVertical)
				{
					m_NavigationBehaviour.SetEntitiesVertical(elements);
				}
			}
			else
			{
				m_NavigationBehaviour.SetEntitiesHorizontal(elements);
			}
		}
		else
		{
			m_NavigationBehaviour.SetEntitiesGrid(elements, virtualListLayoutSettingsGrid.ElementsInRow);
		}
		m_FocusSubscription = m_NavigationBehaviour.Focus.Subscribe(OnFocusChanged);
		return m_NavigationBehaviour;
	}

	internal void AddElement(VirtualListElement element)
	{
		if (m_NavigationBehaviour == null)
		{
			return;
		}
		IVirtualListLayoutSettings layoutSettings = m_LayoutSettings;
		if (!(layoutSettings is VirtualListLayoutSettingsGrid))
		{
			if (!(layoutSettings is VirtualListLayoutSettingsHorizontal))
			{
				if (layoutSettings is VirtualListLayoutSettingsVertical)
				{
					m_NavigationBehaviour.AddEntityVertical(element);
				}
			}
			else
			{
				m_NavigationBehaviour.AddEntityHorizontal(element);
			}
		}
		else
		{
			m_NavigationBehaviour.AddEntityGrid(element, elementsInRowCountStays: true);
		}
	}

	internal void InsertElement(int index, VirtualListElement element)
	{
		if (m_NavigationBehaviour == null)
		{
			return;
		}
		IVirtualListLayoutSettings layoutSettings = m_LayoutSettings;
		if (!(layoutSettings is VirtualListLayoutSettingsGrid virtualListLayoutSettingsGrid))
		{
			if (!(layoutSettings is VirtualListLayoutSettingsHorizontal))
			{
				if (layoutSettings is VirtualListLayoutSettingsVertical)
				{
					m_NavigationBehaviour.InsertVertical(index, element);
				}
			}
			else
			{
				m_NavigationBehaviour.InsertHorizontal(index, element);
			}
		}
		else
		{
			int row = index / virtualListLayoutSettingsGrid.ElementsInRow;
			int column = index % virtualListLayoutSettingsGrid.ElementsInRow;
			m_NavigationBehaviour.InsertInGrid(row, column, element, elementsInRowCountStays: true);
		}
	}

	internal void RemoveElement(VirtualListElement element)
	{
		if (m_NavigationBehaviour == null)
		{
			return;
		}
		IVirtualListLayoutSettings layoutSettings = m_LayoutSettings;
		if (!(layoutSettings is VirtualListLayoutSettingsGrid))
		{
			if (layoutSettings is VirtualListLayoutSettingsHorizontal || layoutSettings is VirtualListLayoutSettingsVertical)
			{
				m_NavigationBehaviour.RemoveEntity(element);
			}
		}
		else
		{
			m_NavigationBehaviour.RemoveEntityGrid(element, elementsInRowCountStays: true);
		}
	}

	internal void Clear()
	{
		if (m_NavigationBehaviour != null)
		{
			m_NavigationBehaviour.Clear();
		}
	}

	internal void ResetNavigation()
	{
		m_NavigationBehaviour = null;
		m_FocusSubscription?.Dispose();
		m_FocusSubscription = null;
	}

	public bool CanFocusEntity(IConsoleEntity entity)
	{
		if (entity is VirtualListElement element)
		{
			bool needScrollDown;
			return m_Scroll.ElementIsInScrollZone(element, out needScrollDown);
		}
		return false;
	}

	public void ForceScrollToEntity(IConsoleEntity entity)
	{
		m_Scroll.ForceScrollToElement(entity as VirtualListElement);
	}

	public void ScrollLeft()
	{
		m_Scroll.Scroll(0f - m_ScrollSettings.ConsoleNavigationScrollSpeed);
	}

	public void ScrollRight()
	{
		m_Scroll.Scroll(m_ScrollSettings.ConsoleNavigationScrollSpeed);
	}

	public void ScrollUp()
	{
		m_Scroll.Scroll(0f - m_ScrollSettings.ConsoleNavigationScrollSpeed);
	}

	public void ScrollDown()
	{
		m_Scroll.Scroll(m_ScrollSettings.ConsoleNavigationScrollSpeed);
	}

	public void ScrollInDirection(Vector2 direction)
	{
		if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
		{
			m_Scroll.Scroll(m_ScrollSettings.ConsoleNavigationScrollSpeed * Vector2.Dot(Vector2.right, direction));
		}
		else
		{
			m_Scroll.Scroll(m_ScrollSettings.ConsoleNavigationScrollSpeed * Vector2.Dot(Vector2.down, direction));
		}
	}

	private void OnFocusChanged(IConsoleEntity entity)
	{
		VirtualListElement element = entity as VirtualListElement;
		if (element != null && !m_Scroll.ElementIsInScrollZone(element, out var _))
		{
			m_Scroll.ForceScrollToElement(element);
			DelayedInvoker.InvokeInFrames(delegate
			{
				element.SetFocus(value: true);
			}, 1);
		}
	}
}
