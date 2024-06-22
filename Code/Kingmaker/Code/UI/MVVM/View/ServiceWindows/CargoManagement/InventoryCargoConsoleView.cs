using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement;

public class InventoryCargoConsoleView : InventoryCargoView
{
	[SerializeField]
	protected ConsoleHint m_CargoHint;

	[SerializeField]
	protected ConsoleHint m_CargoLeftHint;

	[SerializeField]
	protected ConsoleHint m_CargoRightHint;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	[SerializeField]
	private ConsoleHint m_CloseHint;

	[SerializeField]
	protected TextMeshProUGUI m_ChangeCargoViewText;

	[SerializeField]
	private ConsoleHint m_SortingHint;

	private bool IsSortBinded;

	public readonly ReactiveCommand OnNeedRefocus = new ReactiveCommand();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_ChangeCargoViewText != null)
		{
			m_ChangeCargoViewText.text = UIStrings.Instance.CargoTexts.Cargo;
		}
		if (base.ViewModel.CargoViewType != InventoryCargoViewType.Vendor)
		{
			AddDisposable(base.ViewModel.SelectedCargo.Subscribe(delegate
			{
				ChangeCargoView();
			}));
		}
		if (m_CargoZoneView is CargoDetailedZoneConsoleView cargoDetailedZoneConsoleView)
		{
			AddDisposable(ObservableExtensions.Subscribe(cargoDetailedZoneConsoleView.OnHideSlot, delegate
			{
				HandleRemoveCargo();
			}));
		}
		base.ViewModel.IsCargoDetailedZone.Value = false;
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		IsSortBinded = false;
	}

	public ConsoleHint GetHint()
	{
		if (m_CargoButtonText != null)
		{
			m_CargoButtonText.text = UIStrings.Instance.CargoTexts.Cargo;
		}
		if (m_ListButtonText != null)
		{
			m_ListButtonText.text = UIStrings.Instance.CargoTexts.CargoList;
		}
		return m_CargoHint;
	}

	public ConsoleNavigationBehaviour GetCurrentStateNavigation()
	{
		if (!base.IsBinded)
		{
			return null;
		}
		if (!base.ViewModel.IsCargoDetailedZone.Value)
		{
			return GetCargoNavigation();
		}
		return GetDetailedCargoZoneNavigation();
	}

	public ConsoleNavigationBehaviour GetDetailedCargoZoneNavigation()
	{
		if (m_CargoZoneView is CargoDetailedZoneConsoleView cargoDetailedZoneConsoleView)
		{
			return cargoDetailedZoneConsoleView.GetNavigation();
		}
		return null;
	}

	public void AddInput(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		if (m_CargoZoneView is CargoDetailedZoneConsoleView cargoDetailedZoneConsoleView)
		{
			cargoDetailedZoneConsoleView.AddInput(inputLayer, enabledHints);
		}
		if ((bool)m_CargoLeftHint && (bool)m_CargoRightHint)
		{
			AddDisposable(m_CargoLeftHint.Bind(inputLayer.AddButton(delegate
			{
				ChangeCargoView();
			}, 14, enabledHints)));
			AddDisposable(m_CargoRightHint.Bind(inputLayer.AddButton(delegate
			{
				ChangeCargoView();
			}, 15, enabledHints)));
		}
		if (base.ViewModel != null && (bool)m_SortingHint && !IsSortBinded)
		{
			AddDisposable(m_SortingHint.Bind(inputLayer.AddButton(ShowSortingMenu, 17, base.ViewModel.HasVisibleCargo.And(enabledHints).ToReactiveProperty(), InputActionEventType.ButtonJustReleased)));
			IsSortBinded = true;
		}
	}

	public bool TryRebindCargoZone(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		if (m_CargoZoneView is CargoDetailedZoneConsoleView cargoDetailedZoneConsoleView)
		{
			cargoDetailedZoneConsoleView.AddInput(inputLayer, enabledHints);
			if (base.ViewModel != null && (bool)m_SortingHint && !IsSortBinded)
			{
				AddDisposable(m_SortingHint.Bind(inputLayer.AddButton(ShowSortingMenu, 17, base.ViewModel.IsCargoDetailedZone.And(base.ViewModel.HasVisibleCargo.And(enabledHints)).ToReactiveProperty(), InputActionEventType.ButtonJustReleased)));
				IsSortBinded = true;
			}
			return true;
		}
		return false;
	}

	public List<IDisposable> AddInputDisposable(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		List<IDisposable> list = new List<IDisposable>();
		if (m_CargoZoneView is CargoDetailedZoneConsoleView cargoDetailedZoneConsoleView)
		{
			list.AddRange(cargoDetailedZoneConsoleView.AddInputDisposable(inputLayer, enabledHints));
		}
		return list;
	}

	public IDisposable AddInputSorting(InputLayer inputLayer, ReactiveProperty<bool> enabledHints)
	{
		return m_SortingHint.Bind(inputLayer.AddButton(ShowSortingMenu, 17, base.ViewModel.HasVisibleCargo.And(enabledHints).ToReactiveProperty(), InputActionEventType.ButtonJustReleased));
	}

	public ConsoleNavigationBehaviour GetCargoNavigation()
	{
		return m_VirtualList.GetNavigationBehaviour();
	}

	public void HandleRemoveCargo()
	{
		m_CargoZoneView.ScrollToTop();
		DelayedInvoker.InvokeInFrames(delegate
		{
			OnNeedRefocus?.Execute();
		}, 3);
	}

	public void CreateNavigation()
	{
		GetCurrentStateNavigation();
		CreateInput();
		AddInput(m_InputLayer, base.ViewModel.IsCargoDetailedZone);
		AddDisposable(ObservableExtensions.Subscribe(OnCargoViewChange, delegate
		{
			Del();
		}));
	}

	private void Del()
	{
		DelayedInvoker.InvokeInFrames(OnCargoUpd, 1);
	}

	private void ShowSortingMenu(InputActionEventData data)
	{
		ItemsFilterSearchConsoleView obj = m_CargoZoneView.SearchView as ItemsFilterSearchConsoleView;
		if ((object)obj == null || !obj.IsActive)
		{
			TooltipHelper.HideTooltip();
			m_SorterDropdown.SetState(value: true);
		}
	}

	private void OnCargoUpd()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = (GridConsoleNavigationBehaviour)GetCurrentStateNavigation();
		m_NavigationBehaviour.FocusOnFirstValidEntity();
	}

	private void CreateInput()
	{
		m_InputLayer = new InputLayer
		{
			ContextName = "CargoReceived"
		};
		AddDisposable(m_CloseHint.Bind(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.Close();
		}, 9, InputActionEventType.ButtonJustReleased)));
		m_CloseHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
		AddDisposable(GetHint().Bind(m_InputLayer.AddButton(delegate
		{
			ChangeCargoView();
		}, 18)));
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}
}
