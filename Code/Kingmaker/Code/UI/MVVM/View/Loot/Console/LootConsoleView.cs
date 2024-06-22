using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.Loot;
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

namespace Kingmaker.Code.UI.MVVM.View.Loot.Console;

public class LootConsoleView : LootView<InventoryCargoConsoleView, LootCollectorConsoleView, InteractionSlotPartConsoleView, PlayerStashConsoleView, InventoryStashConsoleView>, ICullFocusHandler, ISubscriber
{
	[Header("Console")]
	[SerializeField]
	private TooltipPlaces m_StashTooltipPlaces;

	[SerializeField]
	private TooltipPlaces m_CargoTooltipPlaces;

	[SerializeField]
	private TooltipPlaces m_CenterTooltipPlaces;

	[SerializeField]
	private CanvasSortingComponent m_SortingComponent;

	[SerializeField]
	private ConsoleHintsWidget m_MiddleHintsWidget;

	[SerializeField]
	private ConsoleHintsWidget m_LeftHintsWidget;

	[SerializeField]
	private ConsoleHintsWidget m_RightHintsWidget;

	[SerializeField]
	protected RectTransform m_LeftCanvas;

	[SerializeField]
	protected RectTransform m_RightCanvas;

	[SerializeField]
	protected RectTransform m_CenterCanvas;

	[Header("Animation Settings")]
	[SerializeField]
	private float m_FocusedRotation = 5f;

	[SerializeField]
	private float m_FocusTweenTime = 0.5f;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private ConsoleNavigationBehaviour m_CargoNavigation;

	private readonly BoolReactiveProperty m_ShowTooltip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasTooltip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasItem = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanTransfer = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_InventoryFocus = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CargoFocus = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_LootFocus = new BoolReactiveProperty();

	private IConsoleEntity m_CurrentEntity;

	private IConsoleHint m_MiddleConfirmHint;

	private IConsoleHint m_LeftConfirmHint;

	private IConsoleHint m_RightConfirmHint;

	private ItemSlotsGroupType m_LastFocusGroup = ItemSlotsGroupType.Loot;

	private Vector3 m_LeftCanvasInitPosition;

	private Vector3 m_RightCanvasInitPosition;

	private TooltipConfig m_MainTooltipConfig = new TooltipConfig
	{
		InfoCallConsoleMethod = InfoCallConsoleMethod.None
	};

	private TooltipConfig m_CompareTooltipConfig = new TooltipConfig
	{
		InfoCallConsoleMethod = InfoCallConsoleMethod.None
	};

	private IDisposable m_UpdateNavigationDelay;

	private IConsoleEntity m_CulledFocus;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_LeftCanvasInitPosition = m_LeftCanvas.anchoredPosition;
		m_RightCanvasInitPosition = m_RightCanvas.anchoredPosition;
		m_MiddleHintsWidget.gameObject.SetActive(value: true);
		m_RightHintsWidget.gameObject.SetActive(value: true);
		m_LeftHintsWidget.gameObject.SetActive(value: true);
		AddDisposable(OnPanelsChanged.Subscribe(UpdateNavigationDelayed));
		if (m_Cargo.IsBinded)
		{
			AddDisposable(ObservableExtensions.Subscribe(m_Cargo.OnCargoViewChange, delegate
			{
				OnCargoViewChange();
			}));
			AddDisposable(ObservableExtensions.Subscribe(m_Cargo.CargoZoneView.OnEnableSlotsChange, delegate
			{
				UpdateNavigationDelayed();
			}));
			AddDisposable(m_Cargo.HasVisibleCargo.Skip(1).Subscribe(delegate
			{
				UpdateNavigationDelayed();
			}));
			AddDisposable(m_Cargo.CargoZoneView.ScrolledEntity.Subscribe(RefocusOnCargo));
		}
		AddDisposable(m_CargoFocus.Skip(1).Subscribe(OnFocusToPanelLeft));
		AddDisposable(m_LootFocus.Skip(1).Subscribe(OnFocusToPanelCenter));
		AddDisposable(m_InventoryFocus.Skip(1).Subscribe(OnFocusToPanelRight));
		AddDisposable(EventBus.Subscribe(this));
		CreateNavigationDelayed();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		DOTween.Kill(m_LeftCanvas);
		DOTween.Kill(m_RightCanvas);
		DOTween.Kill(m_CenterCanvas);
		m_LeftCanvas.anchoredPosition = m_LeftCanvasInitPosition;
		m_RightCanvas.anchoredPosition = m_RightCanvasInitPosition;
		TooltipHelper.HideTooltip();
	}

	private void CreateNavigationDelayed()
	{
		DelayedInvoker.InvokeInFrames(CreateNavigation, 2);
	}

	private void CreateNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		AddDisposable(UniRxExtensionMethods.Subscribe(base.ViewModel.LootUpdated, delegate
		{
			OnEntityFocused(m_NavigationBehaviour.DeepestNestedFocus);
		}));
		AddDisposable(UniRxExtensionMethods.Subscribe(m_Cargo.OnCargoViewChange, delegate
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				OnEntityFocused(m_CargoNavigation.DeepestNestedFocus);
			}, 5);
		}));
		AddDisposable(UniRxExtensionMethods.Subscribe(m_Cargo.OnDetailedCargoShown, delegate
		{
			OnEntityFocused(m_NavigationBehaviour.DeepestNestedFocus);
		}));
		AddNavigation();
		CreateInput();
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnEntityFocused));
	}

	private void AddNavigation()
	{
		IConsoleEntity consoleEntity = null;
		if (m_LootFocus.Value)
		{
			consoleEntity = m_Collector.GetCurrentFocus();
		}
		else if (m_InventoryFocus.Value)
		{
			consoleEntity = m_Inventory.GetCurrentFocus();
		}
		m_NavigationBehaviour.Clear();
		ConsoleNavigationBehaviour consoleNavigationBehaviour = null;
		ConsoleNavigationBehaviour consoleNavigationBehaviour2 = null;
		if (!m_Cargo.IsCargoLocked && ((m_Cargo.IsBinded && base.ViewModel.Mode == LootContextVM.LootWindowMode.PlayerChest) || (m_Cargo.IsBinded && base.ViewModel.LootCollector.Loot.ExtendedView.Value)))
		{
			m_CargoNavigation = m_Cargo.GetCurrentStateNavigation();
			m_NavigationBehaviour.AddEntityHorizontal(m_CargoNavigation);
		}
		if (m_Collector.IsBinded)
		{
			consoleNavigationBehaviour = m_Collector.GetNavigation();
		}
		if (m_PlayerStash.IsBinded)
		{
			consoleNavigationBehaviour = m_PlayerStash.GetNavigation();
		}
		if (m_InteractionSlot.IsBinded)
		{
			consoleNavigationBehaviour = m_InteractionSlot.GetNavigation();
		}
		m_NavigationBehaviour.AddEntityHorizontal(consoleNavigationBehaviour);
		LootContextVM.LootWindowMode mode = base.ViewModel.Mode;
		if (mode == LootContextVM.LootWindowMode.PlayerChest || mode == LootContextVM.LootWindowMode.OneSlot || base.ViewModel.LootCollector.Loot.ExtendedView.Value)
		{
			consoleNavigationBehaviour2 = m_Inventory.GetNavigation();
			m_NavigationBehaviour.AddEntityHorizontal(consoleNavigationBehaviour2);
		}
		IConsoleEntity consoleEntity2 = null;
		IConsoleEntity consoleEntity3 = consoleNavigationBehaviour?.Entities.FirstOrDefault((IConsoleEntity e) => !(e is SimpleConsoleNavigationEntity) && e.IsValid());
		switch (m_LastFocusGroup)
		{
		case ItemSlotsGroupType.Inventory:
			consoleEntity2 = ((consoleNavigationBehaviour2 != null) ? consoleNavigationBehaviour2.Entities.FirstOrDefault((IConsoleEntity e) => e.IsValid()) : consoleEntity3);
			break;
		case ItemSlotsGroupType.Loot:
			if (base.ViewModel.IsOneSlot)
			{
				consoleEntity2 = consoleNavigationBehaviour2?.Entities.FirstOrDefault((IConsoleEntity e) => !(e is SimpleConsoleNavigationEntity) && e.IsValid());
			}
			else
			{
				m_NavigationBehaviour.SetCurrentEntity(consoleNavigationBehaviour);
				consoleEntity2 = consoleEntity3;
			}
			break;
		case ItemSlotsGroupType.Cargo:
			consoleEntity2 = ((m_CargoNavigation != null) ? m_CargoNavigation.Entities.FirstOrDefault((IConsoleEntity e) => e != null) : consoleEntity3);
			break;
		case ItemSlotsGroupType.Unknown:
			if (base.ViewModel.IsOneSlot)
			{
				consoleEntity2 = consoleNavigationBehaviour2?.Entities.FirstOrDefault((IConsoleEntity e) => !(e is SimpleConsoleNavigationEntity) && e.IsValid());
			}
			break;
		}
		m_NavigationBehaviour.FocusOnEntityManual(consoleEntity ?? consoleEntity2);
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "LootConsoleView"
		});
		AddDisposable(m_InputLayer.AddButton(delegate
		{
			Close();
		}, 9));
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip.And(m_LootFocus).ToReactiveProperty(), InputActionEventType.ButtonJustReleased);
		AddDisposable(m_MiddleHintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Information));
		AddDisposable(inputBindStruct);
		if (!base.ViewModel.IsOneSlot)
		{
			InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(ShowContextMenu, 11, m_HasItem.And(m_LootFocus).ToReactiveProperty());
			AddDisposable(m_MiddleHintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.ContextMenu.ContextMenu));
			AddDisposable(inputBindStruct2);
		}
		IReadOnlyReactiveProperty<bool> readOnlyReactiveProperty = m_CanTransfer.Select((bool value) => value || base.ViewModel.IsPlayerStash).And(m_LootFocus).ToReactiveProperty();
		InputBindStruct inputBindStruct3 = m_InputLayer.AddButton(delegate
		{
		}, 8, readOnlyReactiveProperty);
		AddDisposable(m_MiddleConfirmHint = m_MiddleHintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.ActionTexts.MoveItem));
		AddDisposable(inputBindStruct3);
		InputBindStruct inputBindStruct4 = m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip.And(m_CargoFocus).ToReactiveProperty(), InputActionEventType.ButtonJustReleased);
		AddDisposable(m_LeftHintsWidget.BindHint(inputBindStruct4, UIStrings.Instance.CommonTexts.Information));
		AddDisposable(inputBindStruct4);
		if (!base.ViewModel.IsOneSlot)
		{
			InputBindStruct inputBindStruct5 = m_InputLayer.AddButton(ShowContextMenu, 11, m_HasItem.And(m_CargoFocus).ToReactiveProperty());
			AddDisposable(m_LeftHintsWidget.BindHint(inputBindStruct5, UIStrings.Instance.ContextMenu.ContextMenu));
			AddDisposable(inputBindStruct5);
		}
		InputBindStruct inputBindStruct6 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_CanTransfer.And(m_CargoFocus).ToReactiveProperty());
		AddDisposable(m_LeftConfirmHint = m_LeftHintsWidget.BindHint(inputBindStruct6, UIStrings.Instance.ActionTexts.MoveItem));
		AddDisposable(inputBindStruct6);
		InputBindStruct inputBindStruct7 = m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip.And(m_InventoryFocus).ToReactiveProperty(), InputActionEventType.ButtonJustReleased);
		AddDisposable(m_RightHintsWidget.BindHint(inputBindStruct7, UIStrings.Instance.CommonTexts.Information));
		AddDisposable(inputBindStruct7);
		if (!base.ViewModel.IsOneSlot)
		{
			InputBindStruct inputBindStruct8 = m_InputLayer.AddButton(ShowContextMenu, 11, m_HasItem.And(m_InventoryFocus).ToReactiveProperty());
			AddDisposable(m_RightHintsWidget.BindHint(inputBindStruct8, UIStrings.Instance.ContextMenu.ContextMenu));
			AddDisposable(inputBindStruct8);
		}
		InputBindStruct inputBindStruct9 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_CanTransfer.And(m_InventoryFocus).ToReactiveProperty());
		AddDisposable(m_RightConfirmHint = m_RightHintsWidget.BindHint(inputBindStruct9, UIStrings.Instance.ActionTexts.MoveItem));
		AddDisposable(inputBindStruct9);
		m_Cargo.AddInput(m_InputLayer, m_CargoFocus);
		m_Inventory.ItemsFilter.AddInput(m_InputLayer, m_InventoryFocus);
		if (!base.ViewModel.IsOneSlot && !base.ViewModel.IsPlayerStash)
		{
			m_Collector.AddInput(m_InputLayer);
		}
		AddDisposable(m_SortingComponent.PushView());
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	private void ShowContextMenu(InputActionEventData data)
	{
		ReactiveProperty<List<ContextMenuCollectionEntity>> reactiveProperty = (m_CurrentEntity as IItemSlotView)?.SlotVM?.ContextMenu;
		if (reactiveProperty != null)
		{
			((m_CurrentEntity as MonoBehaviour) ?? (m_CurrentEntity as IMonoBehaviour)?.MonoBehaviour).ShowContextMenu(reactiveProperty.Value);
		}
	}

	private void Close()
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

	private void UpdateNavigationDelayed()
	{
		m_UpdateNavigationDelay?.Dispose();
		m_UpdateNavigationDelay = DelayedInvoker.InvokeInFrames(UpdateNavigation, 3);
	}

	private void UpdateNavigation()
	{
		AddNavigation();
	}

	private void OnFocusToPanelLeft(bool value)
	{
		if (value)
		{
			SetLeftPanelFocusState(state: true);
			SetRightPanelFocusState(state: false);
		}
	}

	private void OnFocusToPanelRight(bool value)
	{
		if (value)
		{
			SetLeftPanelFocusState(state: false);
			SetRightPanelFocusState(state: true);
		}
	}

	private void OnFocusToPanelCenter(bool value)
	{
		if (value)
		{
			SetLeftPanelFocusState(state: false);
			SetRightPanelFocusState(state: false);
		}
	}

	private void SetLeftPanelFocusState(bool state)
	{
		Vector3 endValue = (state ? new Vector3(0f, m_FocusedRotation, 0f) : Vector3.zero);
		m_LeftCanvas.DOLocalRotate(endValue, m_FocusTweenTime).SetUpdate(isIndependentUpdate: true);
	}

	private void SetRightPanelFocusState(bool state)
	{
		Vector3 endValue = (state ? new Vector3(0f, 0f - m_FocusedRotation, 0f) : Vector3.zero);
		m_RightCanvas.DOLocalRotate(endValue, m_FocusTweenTime).SetUpdate(isIndependentUpdate: true);
	}

	private void RefocusOnCargo(CargoSlotVM slot)
	{
		if (m_CargoFocus.Value)
		{
			CargoSlotVM slotVM = base.ViewModel.CargoInventory.CargoSlots.FindOrDefault((CargoSlotVM x) => x.CargoEntity == slot.CargoEntity);
			IConsoleEntity a = m_Cargo.GetCurrentStateNavigation().Entities.FirstOrDefault((IConsoleEntity e) => (e as VirtualListElement)?.Data == slotVM);
			DelayedInvoker.InvokeInFrames(delegate
			{
				m_NavigationBehaviour.FocusOnEntityManual(a);
			}, 3);
		}
	}

	private void OnCargoViewChange()
	{
		UpdateNavigationDelayed();
		m_SelectorView.SetNextTab();
		RebindInput();
	}

	private void RebindInput()
	{
		if (m_Cargo.IsCargoDetailed.Value && m_Cargo.TryRebindCargoZone(m_InputLayer, m_CargoFocus))
		{
			m_InputLayer.Bind();
		}
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip.Value = !m_ShowTooltip.Value;
		SetTooltip(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void OnEntityFocused(IConsoleEntity entity)
	{
		m_CurrentEntity = entity;
		if ((m_CurrentEntity as IItemSlotView)?.SlotVM != null)
		{
			ItemSlotVM itemSlotVM = (m_CurrentEntity as IItemSlotView)?.SlotVM;
			m_HasItem.Value = itemSlotVM?.HasItem ?? false;
			m_CanTransfer.Value = itemSlotVM?.CanTransfer ?? false;
		}
		else if ((m_CurrentEntity as VirtualListElement)?.Data != null)
		{
			IVirtualListElementData obj = (m_CurrentEntity as VirtualListElement)?.Data;
			m_HasItem.Value = (obj as CargoSlotVM)?.CargoEntity != null;
			m_CanTransfer.Value = false;
		}
		else
		{
			m_HasItem.Value = false;
			m_CanTransfer.Value = false;
		}
		DefineFocusPanel(entity);
		SetConfirmLabel(entity);
		SetTooltip(entity);
	}

	private void DefineFocusPanel(IConsoleEntity entity)
	{
		if ((entity as IItemSlotView)?.SlotVM != null)
		{
			ItemSlotVM itemSlotVM = (entity as IItemSlotView)?.SlotVM;
			bool flag = entity is CargoSlotConsoleView;
			m_InventoryFocus.Value = (itemSlotVM != null && itemSlotVM.SlotsGroupType == ItemSlotsGroupType.Inventory) || entity is InsertableLootSlotConsoleView;
			m_CargoFocus.Value = (itemSlotVM != null && itemSlotVM.SlotsGroupType == ItemSlotsGroupType.Cargo) || flag;
			m_LootFocus.Value = itemSlotVM != null && itemSlotVM.SlotsGroupType == ItemSlotsGroupType.Loot;
			if (entity != null)
			{
				m_LastFocusGroup = itemSlotVM?.SlotsGroupType ?? ItemSlotsGroupType.Cargo;
			}
			if (m_Cargo.IsBinded && m_LastFocusGroup == ItemSlotsGroupType.Cargo && !m_Cargo.HasVisibleCargo.Value)
			{
				m_LastFocusGroup = ItemSlotsGroupType.Loot;
			}
		}
		else if (entity as CargoSlotConsoleView != null)
		{
			m_InventoryFocus.Value = false;
			m_CargoFocus.Value = true;
			m_LootFocus.Value = false;
			m_LastFocusGroup = ItemSlotsGroupType.Cargo;
			if (m_Cargo.IsBinded && m_LastFocusGroup == ItemSlotsGroupType.Cargo && !m_Cargo.HasVisibleCargo.Value)
			{
				m_LastFocusGroup = ItemSlotsGroupType.Loot;
			}
		}
		else if ((entity as VirtualListElement)?.Data != null)
		{
			_ = (entity as VirtualListElement).Data;
			m_InventoryFocus.Value = false;
			m_CargoFocus.Value = true;
			m_LootFocus.Value = false;
			m_LastFocusGroup = ItemSlotsGroupType.Cargo;
			if (m_Cargo.IsBinded && m_LastFocusGroup == ItemSlotsGroupType.Cargo && !m_Cargo.HasVisibleCargo.Value)
			{
				m_LastFocusGroup = ItemSlotsGroupType.Loot;
			}
		}
	}

	private void SetConfirmLabel(IConsoleEntity entity)
	{
		string label = string.Empty;
		if (base.ViewModel.IsOneSlot)
		{
			if (!(entity is InsertableLootSlotView))
			{
				if (entity is LootSlotConsoleView)
				{
					label = UIStrings.Instance.ContextMenu.TakeOff.Text;
				}
			}
			else
			{
				label = UIStrings.Instance.ContextMenu.Use.Text;
			}
		}
		else if (base.ViewModel.IsPlayerStash)
		{
			if (!(entity is InventorySlotConsoleView))
			{
				if (entity is LootSlotConsoleView)
				{
					label = (m_CanTransfer.Value ? UIStrings.Instance.LootWindow.SendToInventory.Text : UIStrings.Instance.LootWindow.SendToCargo.Text);
				}
			}
			else
			{
				label = UIStrings.Instance.LootWindow.SendToPlayerChest.Text;
			}
		}
		else if (!(entity is InventorySlotConsoleView inventorySlotConsoleView))
		{
			if (entity is LootSlotConsoleView lootSlotConsoleView)
			{
				label = (base.ViewModel.ToInventory(lootSlotConsoleView.Item) ? UIStrings.Instance.LootWindow.SendToCargo.Text : UIStrings.Instance.LootWindow.SendToInventory.Text);
			}
		}
		else
		{
			label = (inventorySlotConsoleView.SlotVM.IsInStash ? UIStrings.Instance.LootWindow.SendToCargo.Text : UIStrings.Instance.LootWindow.SendToInventory.Text);
		}
		m_MiddleConfirmHint?.SetLabel(label);
		m_LeftConfirmHint?.SetLabel(label);
		m_RightConfirmHint?.SetLabel(label);
	}

	private void SetTooltip(IConsoleEntity entity)
	{
		TooltipHelper.HideTooltip();
		UpdateTooltipConfigs(entity);
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
				monoBehaviour.ShowConsoleTooltip(hasTooltipTemplate.TooltipTemplate(), m_NavigationBehaviour, m_MainTooltipConfig, shouldNotHideLittleTooltip: false, showScrollbar: true);
			}
		}
		else if (entity is IHasTooltipTemplates hasTooltipTemplates)
		{
			List<TooltipBaseTemplate> list = hasTooltipTemplates.TooltipTemplates();
			m_HasTooltip.Value = !list.Empty();
			m_CompareTooltipConfig.MaxHeight = ((list.Count > 2) ? 450 : 0);
			if (m_ShowTooltip.Value)
			{
				monoBehaviour.ShowComparativeTooltip(list, m_MainTooltipConfig, m_CompareTooltipConfig, showScrollbar: true);
			}
		}
		else
		{
			m_HasTooltip.Value = false;
		}
	}

	private void UpdateTooltipConfigs(IConsoleEntity currentEntity)
	{
		if (currentEntity is IItemSlotView)
		{
			TooltipPlaces tooltipPlaces;
			if (!m_CargoFocus.Value)
			{
				tooltipPlaces = ((!m_InventoryFocus.Value) ? m_CenterTooltipPlaces : m_StashTooltipPlaces);
			}
			else
			{
				tooltipPlaces = m_CargoTooltipPlaces;
				if (currentEntity is IHasTooltipTemplates hasTooltipTemplates && hasTooltipTemplates.TooltipTemplates().Count <= 1)
				{
					m_MainTooltipConfig = tooltipPlaces.GetCompareTooltipConfig(m_MainTooltipConfig);
					m_CompareTooltipConfig = tooltipPlaces.GetCompareTooltipConfig(m_CompareTooltipConfig);
					return;
				}
			}
			m_MainTooltipConfig = tooltipPlaces.GetMainTooltipConfig(m_MainTooltipConfig);
			m_CompareTooltipConfig = tooltipPlaces.GetCompareTooltipConfig(m_CompareTooltipConfig);
		}
		else
		{
			m_MainTooltipConfig = new TooltipConfig(InfoCallPCMethod.None, InfoCallConsoleMethod.None);
		}
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
