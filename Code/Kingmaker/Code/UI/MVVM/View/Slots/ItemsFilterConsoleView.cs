using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Slots;

public class ItemsFilterConsoleView : ItemsFilterPCView, ICullFocusHandler, ISubscriber
{
	[Header("Console Input")]
	[SerializeField]
	private ConsoleHint m_PreviousFilterHint;

	[SerializeField]
	private ConsoleHint m_NextFilterHint;

	[SerializeField]
	private ConsoleHint m_SortingHint;

	private ItemsFilterType m_FirstFilter;

	private ItemsFilterType m_LastFilter;

	private IConsoleEntity m_CulledEntity;

	public override void Initialize()
	{
		base.Initialize();
		if (!BuildModeUtility.Data.CloudSwitchSettings)
		{
			m_SearchView.Or(null)?.Initialize();
		}
		m_FirstFilter = m_SortedFiltersList.FirstOrDefault();
		m_LastFilter = m_SortedFiltersList.LastOrDefault();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(m_SorterDropdown.IsOn.Skip(1).Subscribe(delegate(bool value)
		{
			EventBus.RaiseEvent(delegate(ICullFocusHandler h)
			{
				if (value)
				{
					h.HandleRemoveFocus();
				}
				else
				{
					h.HandleRestoreFocus();
				}
			});
		}));
	}

	public void AddInput(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		AddDisposable(m_PreviousFilterHint.Bind(inputLayer.AddButton(OnPrevious, 14, enabledHints)));
		AddDisposable(m_NextFilterHint.Bind(inputLayer.AddButton(OnNext, 15, enabledHints)));
		if ((bool)m_SortingHint)
		{
			AddDisposable(m_SortingHint.Bind(inputLayer.AddButton(ShowSortingMenu, 17, enabledHints, InputActionEventType.ButtonJustReleased)));
		}
		(m_SearchView as ItemsFilterSearchConsoleView)?.AddInput(inputLayer, enabledHints);
	}

	public IEnumerable<IDisposable> AddInputDisposable(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		List<IDisposable> list = new List<IDisposable>
		{
			m_PreviousFilterHint.Bind(inputLayer.AddButton(OnPrevious, 14, enabledHints)),
			m_NextFilterHint.Bind(inputLayer.AddButton(OnNext, 15, enabledHints))
		};
		if ((bool)m_SortingHint)
		{
			list.Add(m_SortingHint.Bind(inputLayer.AddButton(ShowSortingMenu, 17, enabledHints, InputActionEventType.ButtonJustReleased)));
		}
		list.Add((m_SearchView as ItemsFilterSearchConsoleView)?.AddInputDisposable(inputLayer, enabledHints));
		return list;
	}

	public void GetNextFilter(InputActionEventData data)
	{
		OnNext(data);
	}

	public void GetPrevFilter(InputActionEventData data)
	{
		OnPrevious(data);
	}

	private void ShowSortingMenu(InputActionEventData data)
	{
		ItemsFilterSearchConsoleView obj = m_SearchView as ItemsFilterSearchConsoleView;
		if ((object)obj == null || !obj.IsActive)
		{
			TooltipHelper.HideTooltip();
			m_SorterDropdown.SetState(value: true);
		}
	}

	private void OnPrevious(InputActionEventData data)
	{
		if (BuildModeUtility.Data.CloudSwitchSettings && base.ViewModel.CurrentFilter.Value == m_FirstFilter)
		{
			base.ViewModel.SetCurrentFilter(m_LastFilter);
			return;
		}
		int value = m_SortedFiltersList.IndexOf(base.ViewModel.CurrentFilter.Value) - 1;
		value = Mathf.Clamp(value, 0, m_SortedFiltersList.Count - 1);
		base.ViewModel.SetCurrentFilter(m_SortedFiltersList.ElementAt(value));
	}

	private void OnNext(InputActionEventData data)
	{
		if (BuildModeUtility.Data.CloudSwitchSettings && base.ViewModel.CurrentFilter.Value == m_LastFilter)
		{
			base.ViewModel.SetCurrentFilter(m_FirstFilter);
			return;
		}
		int value = m_SortedFiltersList.IndexOf(base.ViewModel.CurrentFilter.Value) + 1;
		value = Mathf.Clamp(value, 0, m_SortedFiltersList.Count - 1);
		base.ViewModel.SetCurrentFilter(m_SortedFiltersList.ElementAt(value));
	}

	public void HandleRemoveFocus()
	{
		GridConsoleNavigationBehaviour navigationBehaviour = m_SorterDropdown.GetNavigationBehaviour();
		if (navigationBehaviour != null)
		{
			m_CulledEntity = navigationBehaviour.DeepestNestedFocus;
			navigationBehaviour.UnFocusCurrentEntity();
		}
	}

	public void HandleRestoreFocus()
	{
		if (m_CulledEntity != null)
		{
			m_SorterDropdown.GetNavigationBehaviour()?.FocusOnEntityManual(m_CulledEntity);
		}
		m_CulledEntity = null;
	}
}
