using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.View.Vendor.Console;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorConsoleView : VendorView<InventoryStashConsoleView, InventoryCargoConsoleView, ItemsFilterConsoleView, VendorLevelItemsConsoleView, VendorTransitionWindowConsoleView, VendorReputationForItemWindowConsoleView>
{
	[Header("TooltipPlaces")]
	[SerializeField]
	private TooltipPlaces m_StashTooltipPlaces;

	[SerializeField]
	private TooltipPlaces m_CenterTooltipPlaces;

	[Header("Console")]
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	[SerializeField]
	private ConsoleHint m_NextWindowHint;

	[SerializeField]
	private ConsoleHint m_PrevWindowHint;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private BoolReactiveProperty IsVendorSelected = new BoolReactiveProperty();

	private BoolReactiveProperty IsPlayerStashSelected = new BoolReactiveProperty();

	private BoolReactiveProperty IsReputationSelected = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_ShowTooltip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasTooltip = new BoolReactiveProperty();

	private BoolReactiveProperty m_CanSendToCargo = new BoolReactiveProperty();

	private BoolReactiveProperty m_CanSendToInventory = new BoolReactiveProperty();

	private BoolReactiveProperty m_IsVendorTradeItem = new BoolReactiveProperty();

	private BoolReactiveProperty m_IsVendorBuyItem = new BoolReactiveProperty();

	private BoolReactiveProperty CanSell = new BoolReactiveProperty();

	private BoolReactiveProperty HasCargo = new BoolReactiveProperty();

	private List<IDisposable> m_DisposableBinds = new List<IDisposable>();

	private List<IDisposable> m_DisposableVendorBinds = new List<IDisposable>();

	private List<IDisposable> m_Disposables = new List<IDisposable>();

	private TooltipConfig m_MainTooltipConfig = new TooltipConfig
	{
		InfoCallConsoleMethod = InfoCallConsoleMethod.None
	};

	private TooltipConfig m_CompareTooltipConfig = new TooltipConfig
	{
		InfoCallConsoleMethod = InfoCallConsoleMethod.None
	};

	private IDisposable m_UpdateBindsDelay;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		DisposeReputationBind();
		DisposeVendorBind();
		VendorReputationPartConsoleView vendorReputationPartConsoleView = m_VendorReputationPartPCView as VendorReputationPartConsoleView;
		CreateNavigation();
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnEntityFocused));
		AddDisposable(base.ViewModel.ActiveTab.Subscribe(delegate
		{
			UpdateNavigation();
		}));
		AddDisposable(ObservableExtensions.Subscribe(vendorReputationPartConsoleView.CargoConsoleView.OnCargoViewChange, delegate
		{
			UpdateNavigation();
		}));
		AddDisposable(base.ViewModel.CargoCollectionChange.Subscribe(UpdateNavigation));
		AddDisposable(ObservableExtensions.Subscribe(vendorReputationPartConsoleView.CargoConsoleView.CargoZoneView.OnEnableSlotsChange, delegate
		{
			UpdateNavigation();
		}));
		AddDisposable(ObservableExtensions.Subscribe(vendorReputationPartConsoleView.CargoConsoleView.CargoZoneView.NeedRefocus, delegate
		{
			Refocus();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_VendorTradePartView.OnUpdateSlots, delegate
		{
			UpdateNavigation();
		}));
		AddDisposable(base.ViewModel.ActiveTab.Subscribe(delegate
		{
			UpdateBindsDelayed();
		}));
		AddDisposable(ObservableExtensions.Subscribe(vendorReputationPartConsoleView.CargoConsoleView.OnCargoViewChange, delegate
		{
			UpdateBindsDelayed();
		}));
		if (m_VendorReputationPartPCView is VendorReputationPartConsoleView vendorReputationPartConsoleView2)
		{
			AddDisposable(vendorReputationPartConsoleView2.OnNeedRefocus.Subscribe(NeededRefocus));
		}
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Vendor Console View"
		});
		VendorTradePartConsoleView vendorTradePartConsoleView = m_VendorTradePartView as VendorTradePartConsoleView;
		if (vendorTradePartConsoleView != null)
		{
			m_NavigationBehaviour.AddEntityGrid(vendorTradePartConsoleView.GetNavigation());
		}
		m_NavigationBehaviour.AddEntityGrid(m_StashView.GetNavigation());
		m_NavigationBehaviour.FocusOnFirstValidEntity();
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	private void UpdateBindsDelayed()
	{
		m_UpdateBindsDelay?.Dispose();
		m_UpdateBindsDelay = DelayedInvoker.InvokeInFrames(UpdateBinds, 3);
	}

	private void UpdateNavigation()
	{
		m_NavigationBehaviour.Clear();
		if (base.ViewModel.ActiveTab.Value == VendorWindowsTab.Trade)
		{
			VendorTradePartConsoleView vendorTradePartConsoleView = m_VendorTradePartView as VendorTradePartConsoleView;
			if (vendorTradePartConsoleView != null)
			{
				m_NavigationBehaviour.AddEntityGrid(vendorTradePartConsoleView.GetNavigation());
			}
			m_NavigationBehaviour.AddEntityGrid(m_StashView.GetNavigation());
			m_NavigationBehaviour.FocusOnFirstValidEntity();
			return;
		}
		VendorReputationPartConsoleView vendorReputationPartConsoleView = m_VendorReputationPartPCView as VendorReputationPartConsoleView;
		if (vendorReputationPartConsoleView != null)
		{
			m_NavigationBehaviour.AddEntityGrid(vendorReputationPartConsoleView.GetNavigation());
		}
		m_NavigationBehaviour.AddEntityGrid(m_StashView.GetNavigation());
		if ((bool)vendorReputationPartConsoleView)
		{
			m_NavigationBehaviour.FocusOnEntityManual(vendorReputationPartConsoleView.m_CurrentFocus);
		}
		else
		{
			m_NavigationBehaviour.FocusOnFirstValidEntity();
		}
	}

	private void UpdateBinds()
	{
		m_InputLayer.Unbind();
		if (base.ViewModel.ActiveTab.Value == VendorWindowsTab.Trade)
		{
			CreateInput();
		}
		else
		{
			VendorReputationPartConsoleView vendorReputationPartConsoleView = m_VendorReputationPartPCView as VendorReputationPartConsoleView;
			if (vendorReputationPartConsoleView != null)
			{
				m_Disposables.ForEach(delegate(IDisposable d)
				{
					d.Dispose();
				});
				m_Disposables.Clear();
				m_Disposables.Add(vendorReputationPartConsoleView.CanSell.Subscribe(delegate(bool val)
				{
					CanSell.Value = val;
				}));
				m_Disposables.Add(vendorReputationPartConsoleView.HasVisibleCargo.Subscribe(delegate(bool val)
				{
					HasCargo.Value = val;
				}));
				m_Disposables.Add(vendorReputationPartConsoleView.CargoConsoleView.AddInputSorting(m_InputLayer, IsReputationSelected));
				if (!vendorReputationPartConsoleView.CargoConsoleView.IsCargoDetailed.Value)
				{
					UpdateReputationBind();
				}
				else
				{
					DisposeVendorBind();
					DisposeReputationBind();
					InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
					{
						vendorReputationPartConsoleView.CargoConsoleView.ChangeCargoView();
					}, 18, InputActionEventType.ButtonJustReleased);
					m_DisposableBinds.Add(vendorReputationPartConsoleView.CargoConsoleView.GetHint().Bind(inputBindStruct));
					m_DisposableBinds.Add(inputBindStruct);
					m_DisposableBinds.AddRange(vendorReputationPartConsoleView.CargoConsoleView.AddInputDisposable(m_InputLayer));
					CreateInput();
				}
			}
		}
		m_InputLayer.Bind();
	}

	private void UpdateReputationBind()
	{
		DisposeReputationBind();
		VendorReputationPartConsoleView vendorReputationView = m_VendorReputationPartPCView as VendorReputationPartConsoleView;
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			vendorReputationView.SellCargo();
		}, 10, CanSell, InputActionEventType.ButtonJustLongPressed);
		m_DisposableBinds.Add(vendorReputationView.GetSellHint().Bind(inputBindStruct));
		m_DisposableBinds.Add(inputBindStruct);
		InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(delegate
		{
			vendorReputationView.HandleContextMenu();
		}, 11, HasCargo.And(IsPlayerStashSelected.Not()).ToReactiveProperty(), InputActionEventType.ButtonJustReleased);
		ConsoleHint selectContextMenuHint = vendorReputationView.GetSelectContextMenuHint();
		m_DisposableBinds.Add(selectContextMenuHint.Bind(inputBindStruct2));
		m_DisposableBinds.Add(inputBindStruct2);
		selectContextMenuHint.SetLabel(UIStrings.Instance.Vendor.CargoSelectingMenu);
		InputBindStruct inputBindStruct3 = m_InputLayer.AddButton(delegate
		{
			vendorReputationView.SetUnrelevantToggle();
		}, 10, IsPlayerStashSelected.Not().ToReactiveProperty(), InputActionEventType.ButtonJustReleased);
		m_DisposableBinds.Add(vendorReputationView.GetUnrelevantHint().Bind(inputBindStruct3));
		m_DisposableBinds.Add(inputBindStruct3);
		InputBindStruct inputBindStruct4 = m_InputLayer.AddButton(delegate
		{
			vendorReputationView.CargoConsoleView.ChangeCargoView();
		}, 18, InputActionEventType.ButtonJustReleased);
		m_DisposableBinds.Add(vendorReputationView.CargoConsoleView.GetHint().Bind(inputBindStruct4));
		m_DisposableBinds.Add(inputBindStruct4);
		CreateInput();
	}

	private void DisposeReputationBind()
	{
		m_DisposableBinds.ForEach(delegate(IDisposable d)
		{
			d?.Dispose();
		});
		m_DisposableBinds.Clear();
	}

	private void DisposeVendorBind()
	{
		m_DisposableVendorBinds.ForEach(delegate(IDisposable d)
		{
			d?.Dispose();
		});
		m_DisposableVendorBinds.Clear();
	}

	private void Refocus()
	{
		m_NavigationBehaviour.FocusOnFirstValidEntity();
	}

	private void NeededRefocus()
	{
		if (m_VendorReputationPartPCView is VendorReputationPartConsoleView vendorReputationPartConsoleView)
		{
			m_NavigationBehaviour.FocusOnEntityManual(vendorReputationPartConsoleView.NavigationBehaviour);
		}
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip.Value = !m_ShowTooltip.Value;
		OnEntityFocused(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void HandleTooltip(IConsoleEntity entity)
	{
		TooltipHelper.HideTooltip();
		UpdateTooltipConfigs();
		if (entity == null)
		{
			m_HasTooltip.Value = false;
			return;
		}
		MonoBehaviour monoBehaviour = (entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour;
		if (monoBehaviour == null)
		{
			m_HasTooltip.Value = false;
		}
		else if (entity is SimpleConsoleNavigationEntity simpleConsoleNavigationEntity)
		{
			m_HasTooltip.Value = simpleConsoleNavigationEntity.TooltipTemplate() != null;
			if (m_ShowTooltip.Value)
			{
				MonoBehaviour tooltipPlace = simpleConsoleNavigationEntity.GetTooltipPlace();
				if ((bool)tooltipPlace)
				{
					tooltipPlace.ShowConsoleTooltip(simpleConsoleNavigationEntity.TooltipTemplate(), m_NavigationBehaviour, m_MainTooltipConfig, shouldNotHideLittleTooltip: false, showScrollbar: true);
				}
				else
				{
					monoBehaviour.ShowConsoleTooltip(simpleConsoleNavigationEntity.TooltipTemplate(), m_NavigationBehaviour, m_MainTooltipConfig, shouldNotHideLittleTooltip: false, showScrollbar: true);
				}
			}
		}
		else if (entity is IHasTooltipTemplate hasTooltipTemplate)
		{
			m_HasTooltip.Value = hasTooltipTemplate.TooltipTemplate() != null;
			if (m_ShowTooltip.Value)
			{
				monoBehaviour.ShowConsoleTooltip(hasTooltipTemplate.TooltipTemplate(), m_NavigationBehaviour, m_MainTooltipConfig, shouldNotHideLittleTooltip: false, showScrollbar: true);
			}
		}
		else if (entity is IHasTooltipTemplates hasTooltipTemplates)
		{
			List<TooltipBaseTemplate> list = hasTooltipTemplates.TooltipTemplates();
			m_HasTooltip.Value = list.Count > 0;
			if (m_HasTooltip.Value && m_ShowTooltip.Value)
			{
				if (list.Count > 1)
				{
					m_CompareTooltipConfig.MaxHeight = ((list.Count > 2) ? 450 : 0);
					monoBehaviour.ShowComparativeTooltip(hasTooltipTemplates.TooltipTemplates(), m_MainTooltipConfig, m_CompareTooltipConfig, showScrollbar: true);
				}
				else
				{
					monoBehaviour.ShowConsoleTooltip(list.LastOrDefault(), m_NavigationBehaviour, m_MainTooltipConfig, shouldNotHideLittleTooltip: false, showScrollbar: true);
				}
			}
		}
		else
		{
			m_HasTooltip.Value = false;
		}
	}

	private void UpdateTooltipConfigs()
	{
		TooltipPlaces tooltipPlaces = (IsPlayerStashSelected.Value ? m_StashTooltipPlaces : m_CenterTooltipPlaces);
		m_MainTooltipConfig = tooltipPlaces.GetMainTooltipConfig(m_MainTooltipConfig);
		m_CompareTooltipConfig = tooltipPlaces.GetCompareTooltipConfig(m_CompareTooltipConfig);
	}

	private void CreateInput()
	{
		DisposeVendorBind();
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			OnDeclineClick();
		}, 9);
		m_DisposableVendorBinds.Add(m_HintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.CloseWindow));
		m_DisposableVendorBinds.Add(inputBindStruct);
		InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(delegate
		{
			SetNextTab();
		}, 14, IsPlayerStashSelected.Not().ToReactiveProperty());
		m_DisposableVendorBinds.Add(m_PrevWindowHint.Bind(inputBindStruct2));
		m_DisposableVendorBinds.Add(inputBindStruct2);
		InputBindStruct inputBindStruct3 = m_InputLayer.AddButton(delegate
		{
			SetNextTab();
		}, 15, IsPlayerStashSelected.Not().ToReactiveProperty());
		m_DisposableVendorBinds.Add(m_NextWindowHint.Bind(inputBindStruct3));
		m_DisposableVendorBinds.Add(inputBindStruct3);
		InputBindStruct inputBindStruct4 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_IsVendorBuyItem, InputActionEventType.ButtonJustReleased);
		m_DisposableVendorBinds.Add(m_HintsWidget.BindHint(inputBindStruct4, UIStrings.Instance.ContextMenu.Buy));
		m_DisposableVendorBinds.Add(inputBindStruct4);
		InputBindStruct inputBindStruct5 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_CanSendToCargo, InputActionEventType.ButtonJustReleased);
		m_DisposableVendorBinds.Add(m_HintsWidget.BindHint(inputBindStruct5, UIStrings.Instance.LootWindow.SendToCargo));
		m_DisposableVendorBinds.Add(inputBindStruct5);
		InputBindStruct inputBindStruct6 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_CanSendToInventory, InputActionEventType.ButtonJustReleased);
		m_DisposableVendorBinds.Add(m_HintsWidget.BindHint(inputBindStruct6, UIStrings.Instance.LootWindow.SendToInventory));
		m_DisposableVendorBinds.Add(inputBindStruct6);
		InputBindStruct inputBindStruct7 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_IsVendorTradeItem, InputActionEventType.ButtonJustReleased);
		m_DisposableVendorBinds.Add(m_HintsWidget.BindHint(inputBindStruct7, UIStrings.Instance.CommonTexts.Select));
		m_DisposableVendorBinds.Add(inputBindStruct7);
		InputBindStruct inputBindStruct8 = m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased);
		m_DisposableVendorBinds.Add(m_HintsWidget.BindHint(inputBindStruct8, UIStrings.Instance.CommonTexts.Information, ConsoleHintsWidget.HintPosition.Left));
		m_DisposableVendorBinds.Add(inputBindStruct8);
		m_DisposableVendorBinds.AddRange(m_StashView.ItemsFilter.AddInputDisposable(m_InputLayer, IsPlayerStashSelected));
	}

	private void OnEntityFocused(IConsoleEntity currentFocus)
	{
		InventorySlotConsoleView inventorySlotConsoleView = currentFocus as InventorySlotConsoleView;
		bool flag = (bool)inventorySlotConsoleView && inventorySlotConsoleView.SlotVM.SlotsGroupType == ItemSlotsGroupType.Cargo;
		bool flag2 = (bool)inventorySlotConsoleView && inventorySlotConsoleView.Item != null;
		m_IsVendorTradeItem.Value = currentFocus is VendorCargoSlotConsoleView vendorCargoSlotConsoleView && vendorCargoSlotConsoleView.CanConfirmClick();
		m_CanSendToCargo.Value = (bool)inventorySlotConsoleView && inventorySlotConsoleView.SlotVM.IsInStash && flag2;
		m_CanSendToInventory.Value = flag && flag2 && !inventorySlotConsoleView.SlotVM.IsTrash.Value;
		m_IsVendorBuyItem.Value = currentFocus is VendorSlotConsoleView vendorSlotConsoleView && vendorSlotConsoleView.CanConfirmClick();
		SetupNewFilterSelected(currentFocus);
		ScrollToObject(currentFocus);
		HandleTooltip(currentFocus);
	}

	private void SetupNewFilterSelected(IConsoleEntity currentFocus)
	{
		InventorySlotConsoleView inventorySlotConsoleView = currentFocus as InventorySlotConsoleView;
		if ((bool)inventorySlotConsoleView && inventorySlotConsoleView.SlotVM.SlotsGroupType == ItemSlotsGroupType.Inventory && m_VendorTabNavigation.CurrentTab.Value == VendorWindowsTab.Trade)
		{
			IsVendorSelected.Value = false;
			IsReputationSelected.Value = false;
			IsPlayerStashSelected.Value = true;
		}
		else if ((bool)inventorySlotConsoleView && inventorySlotConsoleView.SlotVM.SlotsGroupType == ItemSlotsGroupType.Inventory && m_VendorTabNavigation.CurrentTab.Value == VendorWindowsTab.Reputation)
		{
			IsVendorSelected.Value = false;
			IsReputationSelected.Value = false;
			IsPlayerStashSelected.Value = true;
		}
		else if (m_VendorTabNavigation.CurrentTab.Value == VendorWindowsTab.Trade)
		{
			IsVendorSelected.Value = true;
			IsReputationSelected.Value = false;
			IsPlayerStashSelected.Value = false;
		}
		else if (m_VendorTabNavigation.CurrentTab.Value == VendorWindowsTab.Reputation)
		{
			IsVendorSelected.Value = false;
			IsReputationSelected.Value = true;
			IsPlayerStashSelected.Value = false;
		}
	}

	private void ScrollToObject(IConsoleEntity entity)
	{
		if (entity is VendorSlotConsoleView vendorSlotConsoleView)
		{
			m_VendorTradePartView.m_ScrollRect.EnsureVisible(vendorSlotConsoleView.transform as RectTransform, 100f);
		}
	}

	private void SetNextTab()
	{
		m_SelectorView.SetNextTab();
		m_VendorTabNavigation.SetNextTab();
	}

	protected override void DestroyViewImplementation()
	{
		m_Disposables.ForEach(delegate(IDisposable d)
		{
			d.Dispose();
		});
		m_Disposables.Clear();
		DisposeVendorBind();
		DisposeReputationBind();
		TooltipHelper.HideTooltip();
		base.DestroyViewImplementation();
	}

	private void OnDeclineClick()
	{
		TooltipHelper.HideTooltip();
		if (m_HasTooltip.Value && m_ShowTooltip.Value)
		{
			m_ShowTooltip.Value = false;
		}
		else
		{
			base.ViewModel.Close();
		}
	}
}
