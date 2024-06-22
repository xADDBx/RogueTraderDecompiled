using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility.CanvasSorting;
using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement;

public class CargoManagementConsoleView : CargoManagementBaseView<InventoryStashConsoleView, InventoryCargoConsoleView>, ICullFocusHandler, ISubscriber
{
	[SerializeField]
	private CanvasSortingComponent m_SortingComponent;

	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private readonly BoolReactiveProperty m_HasTooltip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_ShowTooltip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsCargoSelected = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanSendToCargo = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanSendToInventory = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasContextMenu = new BoolReactiveProperty();

	private bool m_LastFocusCargo;

	private IDisposable m_DetailedCargoSub;

	private IDisposable m_UpdateNavigationDelay;

	private IConsoleEntity m_CulledFocus;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateNavigationDelayed();
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DestroyViewImplementation()
	{
		m_DetailedCargoSub?.Dispose();
		base.DestroyViewImplementation();
	}

	private void CreateNavigationDelayed()
	{
		DelayedInvoker.InvokeInFrames(CreateNavigation, 1);
	}

	private void CreateNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "CargoConsoleView"
		}, new BoolReactiveProperty(initialValue: true));
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnEntityFocused));
		ConsoleNavigationBehaviour item = new GridConsoleNavigationBehaviour();
		if (!Game.Instance.Player.CargoState.LockTransferFromCargo)
		{
			item = m_CargoView.GetCargoNavigation();
		}
		GridConsoleNavigationBehaviour navigation = m_StashView.GetNavigation();
		m_NavigationBehaviour.SetEntitiesHorizontal(new List<IConsoleNavigationEntity> { item, navigation });
		m_NavigationBehaviour.FocusOnEntityManual(navigation);
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(delegate(IConsoleEntity value)
		{
			SetTooltip(value);
		}));
		CreateInput();
		AddDisposable(ObservableExtensions.Subscribe(m_CargoView.OnDetailedCargoShown, delegate
		{
			OnCargoDetailedShown();
		}));
		AddDisposable(m_CargoView.HasVisibleCargo.Skip(1).Subscribe(delegate
		{
			UpdateNavigationDelayed();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_CargoView.OnCargoViewChange, delegate
		{
			UpdateNavigationDelayed();
		}));
		AddDisposable(m_CargoView.CargoZoneView.ScrolledEntity.Subscribe(RefocusOnCargo));
		AddDisposable(m_CargoView.OnNeedRefocus.Subscribe(Refocus));
	}

	private void CreateInput()
	{
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			Close();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased), UIStrings.Instance.CommonTexts.Information));
		AddDisposable(m_CargoView.GetHint().Bind(m_InputLayer.AddButton(delegate
		{
			m_CargoView.ChangeCargoView();
		}, 18, base.ViewModel.InventoryCargoVM.HasVisibleCargo)));
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(ShowContextMenu, 10, m_HasContextMenu, InputActionEventType.ButtonJustReleased);
		AddDisposable(m_HintsWidget.BindHint(inputBindStruct, UIStrings.Instance.ContextMenu.ContextMenu));
		AddDisposable(inputBindStruct);
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnConfirm();
		}, 8, m_CanSendToInventory), UIStrings.Instance.LootWindow.SendToInventory));
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			OnConfirm();
		}, 8, m_CanSendToCargo), UIStrings.Instance.LootWindow.SendToCargo));
		m_StashView.ItemsFilter.AddInput(m_InputLayer, m_IsCargoSelected.Not().ToReactiveProperty());
		RebindSearchInput();
		AddDisposable(m_SortingComponent.PushView());
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	private void RefocusOnCargo(CargoSlotVM slot)
	{
		CargoSlotVM slotVM = base.ViewModel.InventoryCargoVM.CargoSlots.FindOrDefault((CargoSlotVM x) => x.CargoEntity == slot.CargoEntity);
		IConsoleEntity entity = m_CargoView.GetCurrentStateNavigation().Entities.FirstOrDefault((IConsoleEntity e) => (e as VirtualListElement)?.Data == slotVM);
		m_NavigationBehaviour.FocusOnEntityManual(entity);
	}

	private void Refocus()
	{
		if (m_CargoView.GetCurrentStateNavigation().Entities.FirstOrDefault((IConsoleEntity x) => x != null) is CargoDetailedConsoleView cargoDetailedConsoleView)
		{
			cargoDetailedConsoleView.NavigationBehaviour.FocusOnFirstValidEntity();
			m_NavigationBehaviour.FocusOnEntityManual(cargoDetailedConsoleView);
		}
		m_NavigationBehaviour.FocusOnFirstValidEntity();
	}

	private void ShowContextMenu(InputActionEventData obj)
	{
		IConsoleEntity deepestNestedFocus = m_NavigationBehaviour.DeepestNestedFocus;
		if (deepestNestedFocus != null)
		{
			((deepestNestedFocus as MonoBehaviour) ?? (deepestNestedFocus as IMonoBehaviour)?.MonoBehaviour).ShowContextMenu((!(deepestNestedFocus is IItemSlotView itemSlotView)) ? null : itemSlotView.SlotVM?.ContextMenu?.Value);
		}
	}

	private void OnEntityFocused(IConsoleEntity entity)
	{
		ItemSlotVM itemSlotVM = (entity as IItemSlotView)?.SlotVM;
		bool flag = entity is InventorySlotConsoleView inventorySlotConsoleView && inventorySlotConsoleView.SlotVM.SlotsGroupType == ItemSlotsGroupType.Cargo;
		if (entity is InventorySlotConsoleView inventorySlotConsoleView2)
		{
			_ = inventorySlotConsoleView2.Item != null;
		}
		else
			_ = 0;
		bool value = entity != null && entity is IItemSlotView itemSlotView && (itemSlotView.SlotVM?.ContextMenu?.Value.Any((ContextMenuCollectionEntity item) => item.IsEnabled)).GetValueOrDefault();
		m_HasContextMenu.Value = value;
		m_CanSendToCargo.Value = itemSlotVM != null && itemSlotVM.CanTransferToCargo && itemSlotVM.IsInStash;
		m_CanSendToInventory.Value = itemSlotVM != null && itemSlotVM.CanTransferToInventory && !itemSlotVM.IsInStash;
		if (m_CargoView.IsCargoDetailed.Value)
		{
			m_IsCargoSelected.Value = flag;
		}
		else
		{
			m_IsCargoSelected.Value = !(entity is InventorySlotConsoleView);
		}
		if (entity != null)
		{
			m_LastFocusCargo = flag || entity is CargoSlotConsoleView;
		}
		SetTooltip(entity);
	}

	private void OnCargoDetailedShown()
	{
		m_DetailedCargoSub?.Dispose();
		m_DetailedCargoSub = ObservableExtensions.Subscribe(m_CargoView.CargoZoneView.OnEnableSlotsChange, delegate
		{
			UpdateNavigationDelayed();
		});
	}

	private void SetTooltip(IConsoleEntity entity)
	{
		TooltipHelper.HideTooltip();
		TooltipConfig tooltipConfig = default(TooltipConfig);
		if (entity is IItemSlotView itemSlotView)
		{
			tooltipConfig = new TooltipConfig(InfoCallPCMethod.None, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, itemSlotView.GetParentContainer(), 0, 0, 0, new List<Vector2>
			{
				new Vector2(0f, 0.5f),
				new Vector2(1f, 0.5f)
			});
		}
		MonoBehaviour monoBehaviour = (entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour;
		if (monoBehaviour == null)
		{
			m_HasTooltip.Value = false;
		}
		else if (entity is IHasTooltipTemplate hasTooltipTemplate)
		{
			m_HasTooltip.Value = hasTooltipTemplate.TooltipTemplate() != null;
			if (m_ShowTooltip.Value)
			{
				monoBehaviour.ShowConsoleTooltip(hasTooltipTemplate.TooltipTemplate(), m_NavigationBehaviour, tooltipConfig);
			}
		}
		else if (entity is IHasTooltipTemplates hasTooltipTemplates)
		{
			List<TooltipBaseTemplate> list = hasTooltipTemplates.TooltipTemplates();
			m_HasTooltip.Value = !list.Empty();
			if (m_ShowTooltip.Value)
			{
				monoBehaviour.ShowComparativeTooltip(list, tooltipConfig, tooltipConfig, showScrollbar: false);
			}
		}
		else
		{
			m_HasTooltip.Value = false;
		}
	}

	private void UpdateNavigationDelayed()
	{
		m_UpdateNavigationDelay?.Dispose();
		m_UpdateNavigationDelay = DelayedInvoker.InvokeInFrames(UpdateNavigation, 1);
	}

	private void UpdateNavigation()
	{
		ConsoleNavigationBehaviour consoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		if (!Game.Instance.Player.CargoState.LockTransferFromCargo)
		{
			consoleNavigationBehaviour = m_CargoView.GetCurrentStateNavigation();
		}
		GridConsoleNavigationBehaviour navigation = m_StashView.GetNavigation();
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.SetEntitiesHorizontal(new List<IConsoleNavigationEntity> { consoleNavigationBehaviour, navigation });
		m_NavigationBehaviour.FocusOnEntityManual((m_LastFocusCargo && m_CargoView.HasVisibleCargo.Value) ? consoleNavigationBehaviour : navigation);
		RebindSearchInput();
	}

	protected void Close()
	{
		TooltipHelper.HideTooltip();
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
	}

	private void OnConfirm()
	{
		OnEntityFocused(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip.Value = !m_ShowTooltip.Value;
		SetTooltip(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void RebindSearchInput()
	{
		m_CargoView.AddInput(m_InputLayer, m_IsCargoSelected);
	}

	public void HandleRemoveFocus()
	{
		m_CulledFocus = m_NavigationBehaviour.DeepestNestedFocus;
		m_NavigationBehaviour.UnFocusCurrentEntity();
	}

	public void HandleRestoreFocus()
	{
		if (m_CulledFocus != null)
		{
			m_NavigationBehaviour.FocusOnEntityManual(m_CulledFocus);
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
		}
		m_CulledFocus = null;
	}
}
