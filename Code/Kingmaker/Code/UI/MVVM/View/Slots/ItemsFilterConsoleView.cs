using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
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

	[SerializeField]
	private ConsoleHint m_ToggleShowUnavailableHint;

	private CompositeDisposable m_DropdownDisposables = new CompositeDisposable();

	private IConsoleEntity m_CulledEntity;

	public override void Initialize()
	{
		base.Initialize();
		if (!BuildModeUtility.Data.CloudSwitchSettings)
		{
			m_SearchView.Or(null)?.Initialize();
		}
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

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_DropdownDisposables.Clear();
	}

	public void AddInput(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> enabledHints = null, ConsoleHintsWidget temp = null)
	{
		foreach (IDisposable item in AddInputDisposable(inputLayer, enabledHints))
		{
			AddDisposable(item);
		}
	}

	public IEnumerable<IDisposable> AddInputDisposable(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		InputBindStruct inputBindStruct = inputLayer.AddButton(base.OnPrevious, 14, enabledHints);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(base.OnNext, 15, enabledHints);
		List<IDisposable> list = new List<IDisposable>
		{
			inputBindStruct,
			m_PreviousFilterHint.Bind(inputBindStruct),
			inputBindStruct2,
			m_NextFilterHint.Bind(inputBindStruct2)
		};
		if ((bool)m_SortingHint)
		{
			InputBindStruct inputBindStruct3 = inputLayer.AddButton(ShowSortingMenu, 17, enabledHints, InputActionEventType.ButtonJustReleased);
			list.Add(m_SortingHint.Bind(inputBindStruct3));
			list.Add(inputBindStruct3);
		}
		if ((bool)m_ToggleShowUnavailableHint && m_ShowToggle)
		{
			AddDisposable(m_SorterDropdown.IsOn.Subscribe(delegate(bool value)
			{
				if (value)
				{
					DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(delegate
					{
						m_DropdownDisposables.Clear();
						InputBindStruct inputBindStruct4 = m_SorterDropdown.InputLayer.AddButton(delegate
						{
							ToggleShowItems();
						}, 11);
						m_DropdownDisposables.Add(m_ToggleShowUnavailableHint.Bind(inputBindStruct4));
						m_DropdownDisposables.Add(inputBindStruct4);
					});
				}
			}));
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

	private void ToggleShowItems()
	{
		m_Toggle.Set(!m_Toggle.IsOn.Value);
	}
}
