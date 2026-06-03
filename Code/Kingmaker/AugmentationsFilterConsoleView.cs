using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker;

public class AugmentationsFilterConsoleView : AugmentationsFiltersPCView
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

	private readonly CompositeDisposable m_DropdownDisposables = new CompositeDisposable();

	private Dictionary<ItemsFilterType, TooltipBaseTemplate> m_FilterTooltips;

	private List<ContextMenuCollectionEntity> m_SortingEntities;

	public readonly ReactiveCommand FilterChanged = new ReactiveCommand();

	private ItemsFilterStrings m_Texts;

	public override void Initialize()
	{
		base.Initialize();
		m_Texts = LocalizedTexts.Instance.ItemsFilter;
		if (!BuildModeUtility.Data.CloudSwitchSettings)
		{
			m_SearchView.Or(null)?.Initialize();
		}
		m_FilterTooltips = new Dictionary<ItemsFilterType, TooltipBaseTemplate>
		{
			{
				ItemsFilterType.AugmentationsAll,
				new TooltipTemplateGlossary("AugmentationsNoFilter")
			},
			{
				ItemsFilterType.AugmentationsArms,
				new TooltipTemplateGlossary("AugmentationsItems")
			}
		};
	}

	public void AddInput(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		AddDisposable(m_PreviousFilterHint.Bind(inputLayer.AddButton(OnPrevious, 14, enabledHints)));
		AddDisposable(m_NextFilterHint.Bind(inputLayer.AddButton(OnNext, 15, enabledHints)));
		if ((bool)m_SortingHint)
		{
			AddDisposable(m_SortingHint.Bind(inputLayer.AddButton(ShowSortingMenu, 17, enabledHints, InputActionEventType.ButtonJustReleased)));
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
						InputBindStruct inputBindStruct = m_SorterDropdown.InputLayer.AddButton(delegate
						{
							ToggleShowItems();
						}, 11);
						m_DropdownDisposables.Add(m_ToggleShowUnavailableHint.Bind(inputBindStruct));
						m_DropdownDisposables.Add(inputBindStruct);
					});
				}
			}));
		}
		(m_SearchView as ItemsFilterSearchConsoleView)?.AddInput(inputLayer, enabledHints);
	}

	public ConsoleHint GetNextFilterHint()
	{
		return m_NextFilterHint;
	}

	public ConsoleHint GetPrevFilterHint()
	{
		return m_PreviousFilterHint;
	}

	public void GetNextFilter(InputActionEventData data)
	{
		OnNext(data);
	}

	public void GetPrevFilter(InputActionEventData data)
	{
		OnPrevious(data);
	}

	private void OnPrevious(InputActionEventData data)
	{
		if (BuildModeUtility.Data.CloudSwitchSettings && base.ViewModel.CurrentFilter.Value == ItemsFilterType.NoFilter)
		{
			base.ViewModel.SetCurrentFilter(ItemsFilterType.NonUsable);
		}
		else if (m_VisibleSearchBar)
		{
			base.ViewModel.SetCurrentFilter(ItemsFilterType.NonUsable);
		}
		else
		{
			if (base.ViewModel.CurrentFilter.Value == ItemsFilterType.NoFilter)
			{
				return;
			}
			int num = (int)(base.ViewModel.CurrentFilter.Value - 1);
			if (num >= 25)
			{
				base.ViewModel.SetCurrentFilter((ItemsFilterType)num);
				if (m_FiltersMap.TryGetValue((ItemsFilterType)num, out var value))
				{
					value.Set(value: true);
				}
			}
		}
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

	private void OnNext(InputActionEventData data)
	{
		if (BuildModeUtility.Data.CloudSwitchSettings && base.ViewModel.CurrentFilter.Value == ItemsFilterType.NonUsable)
		{
			base.ViewModel.SetCurrentFilter(ItemsFilterType.NoFilter);
		}
		else if (m_VisibleSearchBar)
		{
			base.ViewModel.SetCurrentFilter(ItemsFilterType.NoFilter);
		}
		else
		{
			if (base.ViewModel.CurrentFilter.Value == ItemsFilterType.NonUsable)
			{
				return;
			}
			int num = (int)(base.ViewModel.CurrentFilter.Value + 1);
			if (num <= 31)
			{
				base.ViewModel.SetCurrentFilter((ItemsFilterType)num);
				if (m_FiltersMap.TryGetValue((ItemsFilterType)num, out var value))
				{
					value.Set(value: true);
				}
			}
		}
	}

	private void ToggleShowItems()
	{
		m_Toggle.Set(!m_Toggle.IsOn.Value);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_DropdownDisposables.Clear();
	}
}
