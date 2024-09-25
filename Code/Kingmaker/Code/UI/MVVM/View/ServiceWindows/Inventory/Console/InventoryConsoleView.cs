using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Skills;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.CounterWindow;
using Kingmaker.Code.UI.MVVM.VM.SelectorWindow;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.DollRoom;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;
using Kingmaker.UI.MVVM.View.ShipCustomization.Console;
using Kingmaker.UI.Workarounds;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility.CanvasSorting;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;

public class InventoryConsoleView : InventoryBaseView<InventoryStashConsoleView, InventoryDollConsoleView, InventoryEquipSlotConsoleView>, IInventoryHandler, ISubscriber, IEquipItemAutomaticallyHandler, ICullFocusHandler, IContextMenuHandler, ISplitItemHandler, ICounterWindowUIHandler, IInsertItemHandler, IHasDollRoom
{
	[SerializeField]
	private DollRoomTargetController m_DollRoomScaler;

	[Header("Console")]
	[SerializeField]
	private TooltipPlaces m_StashTooltipPlaces;

	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private RectTransform m_TooltipPlaceCenter;

	[Header("Animation Settings")]
	[SerializeField]
	private float m_FocusedRotation = 10f;

	[SerializeField]
	private float m_FocusedOffsetX = 10f;

	[SerializeField]
	private float m_FocusTweenTime = 0.5f;

	[Header("CanvasSorting")]
	[SerializeField]
	private CanvasSortingComponent m_CanvasSortingComponent;

	[SerializeField]
	private List<CanvasSortingComponent> m_NestedComponentsWorkaround = new List<CanvasSortingComponent>();

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private GridConsoleNavigationBehaviour m_NavigationPanelLeft;

	private GridConsoleNavigationBehaviour m_NavigationPanelCenter;

	private GridConsoleNavigationBehaviour m_NavigationPanelRight;

	private readonly BoolReactiveProperty m_FocusOnRightPanel = new BoolReactiveProperty(initialValue: true);

	private readonly BoolReactiveProperty m_HasTooltip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasContextMenu = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanEquip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanChoose = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanSelect = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanFuncAdd = new BoolReactiveProperty();

	private readonly ReactiveProperty<IItemSlotView> m_CurrentItemSlot = new ReactiveProperty<IItemSlotView>();

	private readonly CompositeDisposable m_FocusDisposable = new CompositeDisposable();

	private Vector3 m_LeftCanvasInitPosition;

	private Vector3 m_RightCanvasInitPosition;

	private bool m_ShowTooltipPrevValue;

	private IConsoleHint m_FuncAddHint;

	private IConsoleHint m_TooltipHint;

	private Action m_OnHide;

	private readonly BoolReactiveProperty m_ShowTooltip = new BoolReactiveProperty();

	private TooltipConfig m_MainTooltipConfig = new TooltipConfig
	{
		InfoCallConsoleMethod = InfoCallConsoleMethod.None
	};

	private TooltipConfig m_CompareTooltipConfig = new TooltipConfig
	{
		InfoCallConsoleMethod = InfoCallConsoleMethod.None
	};

	private TooltipConfig m_StatsTooltipConfig = new TooltipConfig
	{
		InfoCallConsoleMethod = InfoCallConsoleMethod.LongRightStickButton
	};

	private IConsoleEntity m_CulledFocus;

	public DollRoomTargetController Controller => m_DollRoomScaler;

	protected override void BindViewImplementation()
	{
		m_LeftCanvasInitPosition = m_LeftCanvas.anchoredPosition;
		m_RightCanvasInitPosition = m_RightCanvas.anchoredPosition;
		base.BindViewImplementation();
		CreateNavigation();
		AddDisposable(m_FocusOnRightPanel.Skip(1).Subscribe(SetRightPanelFocusState));
		AddDisposable(m_FocusOnRightPanel.Not().Skip(1).Subscribe(SetLeftPanelFocusState));
		AddDisposable(base.ViewModel.Unit.Skip(1).Subscribe(delegate
		{
			OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
		}));
		AddDisposable(base.ViewModel.DollVM.InventorySelectorWindowVM.Subscribe(delegate(InventorySelectorWindowVM value)
		{
			SetBusyTooltipMode(value != null);
			if (value == null)
			{
				OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
			}
		}));
		AddDisposable(base.ViewModel.DollVM.ChooseSlotMode.Subscribe(SetBusyTooltipMode));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_OnHide?.Invoke();
		DOTween.Kill(m_LeftCanvas);
		DOTween.Kill(m_RightCanvas);
		m_LeftCanvas.anchoredPosition = m_LeftCanvasInitPosition;
		m_RightCanvas.anchoredPosition = m_RightCanvasInitPosition;
		m_ShowTooltip.Value = false;
	}

	private void CreateNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "InventoryConsoleView"
		});
		GridConsoleNavigationBehaviour navigation = m_StashView.GetNavigation();
		AddDisposable(m_NavigationPanelLeft = new GridConsoleNavigationBehaviour());
		GridConsoleNavigationBehaviour navigationBehaviour = new GridConsoleNavigationBehaviour();
		if (m_LevelClassScoresView is ICharInfoComponentConsoleView charInfoComponentConsoleView)
		{
			charInfoComponentConsoleView.AddInput(ref m_InputLayer, ref m_NavigationPanelLeft, m_ConsoleHintsWidget);
		}
		if (m_SkillsAndWeaponsView is ICharInfoComponentConsoleView charInfoComponentConsoleView2)
		{
			charInfoComponentConsoleView2.AddInput(ref m_InputLayer, ref navigationBehaviour, m_ConsoleHintsWidget);
		}
		(m_SkillsAndWeaponsView as CharInfoSkillsAndWeaponsConsoleView)?.SetFocusChangeAction(OnFocusEntity);
		m_NavigationPanelLeft.AddColumn<GridConsoleNavigationBehaviour>(navigationBehaviour);
		navigationBehaviour.SetCurrentEntity(navigationBehaviour.Entities.FirstOrDefault());
		m_NavigationPanelLeft.SetCurrentEntity(navigationBehaviour);
		AddDisposable(m_NavigationPanelCenter = m_DollView.GetNavigation());
		AddDisposable(m_NavigationPanelRight = new GridConsoleNavigationBehaviour());
		m_NavigationPanelRight = navigation;
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationPanelLeft);
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationPanelCenter);
		m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(m_NavigationPanelRight);
		AddDisposable(m_NavigationPanelLeft.DeepestFocusAsObservable.Skip(1).Subscribe(OnFocusToPanelLeft));
		AddDisposable(m_NavigationPanelCenter.DeepestFocusAsObservable.Skip(1).Subscribe(OnFocusToPanelCenter));
		AddDisposable(m_NavigationPanelRight.DeepestFocusAsObservable.Skip(1).Subscribe(OnFocusToPanelRight));
		AddDisposable(m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusEntity));
		AddDisposable(base.ViewModel.StashVM.CurrentFilter.Subscribe(delegate
		{
			if (m_NavigationBehaviour.Focus.Value == m_NavigationPanelRight)
			{
				m_NavigationPanelRight.FocusOnFirstValidEntity();
				m_NavigationBehaviour.FocusOnEntityManual(m_NavigationPanelRight.DeepestNestedFocus);
				OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
			}
			else
			{
				OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
			}
		}));
		AddDisposable(base.ViewModel.StashVM.CurrentSorter.Subscribe(delegate
		{
			OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
		}));
		SetRightPanelFocusState(state: true);
		CreateInput();
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_NavigationPanelRight.FocusOnFirstValidEntity();
			m_NavigationBehaviour.FocusOnEntityManual(m_NavigationPanelRight.DeepestNestedFocus);
		}, 1);
	}

	private void CreateInput()
	{
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			Close();
		}, 9);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(inputBindStruct);
		InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased);
		AddDisposable(m_TooltipHint = m_ConsoleHintsWidget.BindHint(inputBindStruct2));
		AddDisposable(inputBindStruct2);
		m_TooltipHint.SetLabel(UIStrings.Instance.CommonTexts.Information);
		m_StashView.ItemsFilter.AddInput(m_InputLayer, m_FocusOnRightPanel, m_ConsoleHintsWidget);
		(m_SkillsAndWeaponsView as CharInfoSkillsAndWeaponsConsoleView)?.AddInput(m_InputLayer, m_ConsoleHintsWidget);
		m_DollView.AddInput(m_InputLayer, m_ConsoleHintsWidget, null, m_ShowTooltip);
		if (m_NameAndPortraitPCView is CharInfoNameAndPortraitConsoleView charInfoNameAndPortraitConsoleView)
		{
			charInfoNameAndPortraitConsoleView.AddInput(m_InputLayer, m_ConsoleHintsWidget, m_FocusOnRightPanel.Not().ToReactiveProperty());
		}
		InputBindStruct inputBindStruct3 = m_InputLayer.AddButton(ShowContextMenu, 10, m_HasContextMenu, InputActionEventType.ButtonJustReleased);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.ContextMenu.ContextMenu));
		AddDisposable(inputBindStruct3);
		InputBindStruct inputBindStruct4 = m_InputLayer.AddButton(delegate
		{
			OnFuncAdditionalClick();
		}, 17, m_CanFuncAdd);
		AddDisposable(m_FuncAddHint = m_ConsoleHintsWidget.BindHint(inputBindStruct4));
		AddDisposable(inputBindStruct4);
		InputBindStruct inputBindStruct5 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_CanEquip, InputActionEventType.ButtonJustReleased);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct5, UIStrings.Instance.ContextMenu.Equip));
		AddDisposable(inputBindStruct5);
		InputBindStruct inputBindStruct6 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_CanChoose, InputActionEventType.ButtonJustReleased);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct6, UIStrings.Instance.InventoryScreen.ChooseItem));
		AddDisposable(inputBindStruct6);
		InputBindStruct inputBindStruct7 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_CanSelect, InputActionEventType.ButtonJustReleased);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct7, UIStrings.Instance.CommonTexts.Select));
		AddDisposable(inputBindStruct7);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
		AddDisposable(m_CanvasSortingComponent.PushView());
	}

	private void OnFocusToPanelLeft(IConsoleEntity entity)
	{
		if (entity != null && m_FocusOnRightPanel.Value)
		{
			m_FocusOnRightPanel.Value = false;
		}
	}

	private void OnFocusToPanelRight(IConsoleEntity entity)
	{
		if (entity != null && !m_FocusOnRightPanel.Value)
		{
			m_FocusOnRightPanel.Value = true;
		}
	}

	private void OnFocusToPanelCenter(IConsoleEntity entity)
	{
		if (entity != null)
		{
			if (m_DollView.IsFocusOnRightSlots(entity))
			{
				OnFocusToPanelRight(entity);
			}
			else
			{
				OnFocusToPanelLeft(entity);
			}
		}
	}

	private void SetLeftPanelFocusState(bool state)
	{
		float endValue = (state ? (m_LeftCanvasInitPosition.x + m_FocusedOffsetX) : m_LeftCanvasInitPosition.x);
		Vector3 endValue2 = (state ? new Vector3(0f, m_FocusedRotation, 0f) : Vector3.zero);
		m_LeftCanvas.DOAnchorPosX(endValue, m_FocusTweenTime).SetUpdate(isIndependentUpdate: true);
		m_LeftCanvas.DOLocalRotate(endValue2, m_FocusTweenTime).SetUpdate(isIndependentUpdate: true);
	}

	private void SetRightPanelFocusState(bool state)
	{
		float endValue = (state ? (m_RightCanvasInitPosition.x - m_FocusedOffsetX) : (m_RightCanvasInitPosition.x + m_FocusedOffsetX));
		Vector3 endValue2 = (state ? new Vector3(0f, 0f - m_FocusedRotation, 0f) : Vector3.zero);
		m_RightCanvas.DOAnchorPosX(endValue, m_FocusTweenTime).SetUpdate(isIndependentUpdate: true);
		m_RightCanvas.DOLocalRotate(endValue2, m_FocusTweenTime).SetUpdate(isIndependentUpdate: true);
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip.Value = !m_ShowTooltip.Value;
		OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		m_FocusDisposable.Clear();
		UpdateHintsValues(entity);
		UpdateTooltip(entity);
	}

	private void UpdateTooltip(IConsoleEntity entity)
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
		else if (entity is IHasTooltipTemplate hasTooltipTemplate)
		{
			m_HasTooltip.Value = hasTooltipTemplate.TooltipTemplate() != null;
			TooltipConfig config = (m_NavigationPanelLeft.IsFocused ? m_StatsTooltipConfig : m_MainTooltipConfig);
			if (m_ShowTooltip.Value)
			{
				monoBehaviour.ShowConsoleTooltip(hasTooltipTemplate.TooltipTemplate(), m_NavigationBehaviour, config, shouldNotHideLittleTooltip: true);
			}
		}
		else if (entity is IHasTooltipTemplates hasTooltipTemplates)
		{
			List<TooltipBaseTemplate> list = hasTooltipTemplates.TooltipTemplates();
			m_HasTooltip.Value = list.Count > 0;
			if (m_HasTooltip.Value && m_ShowTooltip.Value)
			{
				m_CompareTooltipConfig.MaxHeight = ((list.Count > 2) ? 450 : 0);
				monoBehaviour.ShowComparativeTooltip(hasTooltipTemplates.TooltipTemplates(), m_MainTooltipConfig, m_CompareTooltipConfig, showScrollbar: true);
			}
		}
		else
		{
			m_HasTooltip.Value = false;
		}
	}

	private void UpdateTooltipConfigs()
	{
		if (m_FocusOnRightPanel.Value && (bool)m_StashTooltipPlaces)
		{
			m_MainTooltipConfig = m_StashTooltipPlaces.GetMainTooltipConfig(m_MainTooltipConfig);
			m_CompareTooltipConfig = m_StashTooltipPlaces.GetCompareTooltipConfig(m_CompareTooltipConfig);
		}
		else
		{
			m_MainTooltipConfig.TooltipPlace = m_TooltipPlaceCenter;
			m_MainTooltipConfig.PriorityPivots = new List<Vector2>
			{
				new Vector2(0.5f, 0.5f)
			};
		}
	}

	private void UpdateHintsValues(IConsoleEntity entity)
	{
		m_CurrentItemSlot.Value = entity as IItemSlotView;
		bool value = m_CurrentItemSlot.Value != null && (m_CurrentItemSlot.Value.SlotVM?.ContextMenu?.Value.Any((ContextMenuCollectionEntity item) => item.IsEnabled)).GetValueOrDefault();
		m_HasContextMenu.Value = value;
		m_CanEquip.Value = !(m_CurrentItemSlot.Value?.SlotVM?.Item.Value?.Blueprint is BlueprintStarshipItem) && (m_CurrentItemSlot.Value?.SlotVM?.IsEquipPossible).GetValueOrDefault() && !m_DollView.IsSlot(entity) && base.ViewModel.Unit.Value.CanBeControlled();
		m_CanChoose.Value = m_DollView.IsSlot(entity);
		m_CanSelect.Value = m_NavigationPanelLeft.IsFocused && ((entity as IConfirmClickHandler)?.CanConfirmClick() ?? false);
		if (entity is IFuncAdditionalClickHandler funcAdditionalClickHandler)
		{
			m_CanFuncAdd.Value = funcAdditionalClickHandler.CanFuncAdditionalClick();
			m_FuncAddHint.SetLabel(funcAdditionalClickHandler.GetFuncAdditionalClickHint());
		}
		else
		{
			m_CanFuncAdd.Value = false;
		}
	}

	private void ShowContextMenu(InputActionEventData obj)
	{
		if (m_CurrentItemSlot.Value is IConsoleEntity consoleEntity)
		{
			((consoleEntity as MonoBehaviour) ?? (consoleEntity as IMonoBehaviour)?.MonoBehaviour).ShowContextMenu(m_CurrentItemSlot.Value.SlotVM?.ContextMenu?.Value);
		}
	}

	private void Close()
	{
		TooltipHelper.HideTooltip();
		if (m_HasTooltip.Value && m_ShowTooltip.Value)
		{
			m_ShowTooltip.Value = false;
			return;
		}
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
	}

	void IInventoryHandler.Refresh()
	{
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	void IInventoryHandler.TryEquip(ItemSlotVM slot)
	{
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	void IInventoryHandler.TryDrop(ItemSlotVM slot)
	{
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	void IInventoryHandler.TryMoveToCargo(ItemSlotVM slot, bool immediately)
	{
		m_FocusDisposable.Add(slot.ItemChanged.Subscribe(delegate
		{
			OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
		}));
	}

	void IInventoryHandler.TryMoveToInventory(ItemSlotVM slot, bool immediately)
	{
	}

	public void HandleEquipItemAutomatically(ItemEntity item)
	{
		m_NavigationBehaviour.FocusOnCurrentEntity();
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

	private void SetBusyTooltipMode(bool isBusy)
	{
		if (isBusy)
		{
			m_ShowTooltipPrevValue = m_ShowTooltip.Value;
			m_ShowTooltip.Value = false;
			TooltipHelper.HideTooltip();
		}
		else
		{
			m_ShowTooltip.Value = m_ShowTooltipPrevValue;
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
			OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
		}
	}

	public void HandleContextMenuRequest(ContextMenuCollection collection)
	{
		SetBusyTooltipMode(collection != null);
	}

	public void HandleSplitItem()
	{
	}

	public void HandleAfterSplitItem(ItemEntity item)
	{
		SetBusyTooltipMode(isBusy: false);
	}

	public void HandleBeforeSplitItem(ItemEntity item, ItemsCollection from, ItemsCollection to)
	{
	}

	public void HandleOpen(CounterWindowType type, ItemEntity item, Action<int> command)
	{
		SetBusyTooltipMode(isBusy: true);
	}

	public void HandleInsertItem(ItemSlot slot)
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
			OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
		}, 1);
	}

	private void OnFuncAdditionalClick()
	{
		(m_NavigationBehaviour.DeepestNestedFocus as IFuncAdditionalClickHandler)?.OnFuncAdditionalClick();
	}

	public void SetCanvasScaler(CanvasScalerWorkaround canvasScaler)
	{
		Controller.CanvasScaler = canvasScaler;
	}

	public void AddSortingComponent(CanvasSortingComponent sortingComponent)
	{
		if (sortingComponent == null)
		{
			return;
		}
		try
		{
			FieldInfo field = m_CanvasSortingComponent.GetType().GetField("m_NestedComponents", BindingFlags.Instance | BindingFlags.NonPublic);
			List<CanvasSortingComponent> list = m_NestedComponentsWorkaround.ToList();
			list.Add(sortingComponent);
			field?.SetValue(m_CanvasSortingComponent, list);
		}
		catch (Exception arg)
		{
			PFLog.UI.Error($"Something went wrong at reflection usage with ex \n{arg}");
		}
	}
}
