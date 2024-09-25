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
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.Tooltips;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.Console;

public class ShipItemsFilterConsoleView : ShipItemsFilterPCView
{
	[Header("Console Input")]
	[SerializeField]
	private ConsoleHint m_PreviousFilterHint;

	[SerializeField]
	private ConsoleHint m_NextFilterHint;

	[SerializeField]
	private ConsoleHint m_SortingHint;

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
				ItemsFilterType.ShipNoFilter,
				new TooltipTemplateGlossary("ShipNoFilter")
			},
			{
				ItemsFilterType.ShipWeapon,
				new TooltipTemplateGlossary("ShipWeapon")
			},
			{
				ItemsFilterType.ShipOther,
				new TooltipTemplateGlossary("ShipOther")
			}
		};
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.CurrentFilter.Subscribe(OnFilterChanged));
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
		else if (base.ViewModel.CurrentFilter.Value != 0)
		{
			base.ViewModel.SetCurrentFilter(base.ViewModel.CurrentFilter.Value - 1);
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

	private void OnFilterChanged(ItemsFilterType filterType)
	{
		foreach (var (owlcatToggle2, tuple2) in m_Filters)
		{
			owlcatToggle2.SetFocused(tuple2.Item1 == filterType);
			owlcatToggle2.Set(tuple2.Item1 == filterType);
			_ = tuple2.Item1;
		}
		FilterChanged.Execute();
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
		else if (base.ViewModel.CurrentFilter.Value != ItemsFilterType.NonUsable)
		{
			base.ViewModel.SetCurrentFilter(base.ViewModel.CurrentFilter.Value + 1);
		}
	}
}
