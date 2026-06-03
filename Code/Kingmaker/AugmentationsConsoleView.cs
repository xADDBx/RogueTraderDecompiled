using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.SelectorWindow;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Augmentations;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.DollRoom;
using Kingmaker.UI.MVVM.View.Bark.Console;
using Kingmaker.UI.MVVM.View.ShipCustomization.Console;
using Kingmaker.UI.Workarounds;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker;

public class AugmentationsConsoleView : AugmentationsBaseView<AugmentationsInventoryStashConsoleView, AugmentationsEquipSlotConsoleView>, IHasDollRoom, IForceRebindOverdriveBlock, ISubscriber, ICullFocusHandler, IUnitOverdriveAugmentHandler
{
	private TooltipConfig m_MainTooltipConfig = new TooltipConfig
	{
		InfoCallConsoleMethod = InfoCallConsoleMethod.None
	};

	private TooltipConfig m_CompareTooltipConfig = new TooltipConfig
	{
		InfoCallConsoleMethod = InfoCallConsoleMethod.None
	};

	[Header("Console")]
	[SerializeField]
	private TooltipPlaces m_StashTooltipPlaces;

	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_Parameters;

	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	protected OwlcatMultiButton m_ExperienceButton;

	[SerializeField]
	private Image m_TooltipPlace;

	[Header("Customization Values")]
	[SerializeField]
	private float m_RotateFactor = 4f;

	[SerializeField]
	private float m_ZoomFactor = 0.2f;

	[SerializeField]
	private float m_ZoomThresholdValue = 0.17f;

	[SerializeField]
	protected InventorySelectorWindowConsoleView m_SelectorWindowView;

	[SerializeField]
	private StarSystemSpaceBarksHolderConsoleView m_StarSystemSpaceBarksHolderConsoleView;

	private readonly Dictionary<AugmentationsSlotVM, AugmentationsEquipSlotConsoleView> m_SlotsMap = new Dictionary<AugmentationsSlotVM, AugmentationsEquipSlotConsoleView>();

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private FloatConsoleNavigationBehaviour m_SlotsNavigation;

	private InputLayer m_AugmentationsInputLayer;

	private bool m_IsCulled;

	private IConsoleEntity m_CulledFocus;

	private InputLayer m_ChooseSlotInputLayer;

	private readonly BoolReactiveProperty m_CanEquip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanChoose = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasOverdrive = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_ShowTooltip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasTooltip = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_HasContextMenu = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_InstallButton = new BoolReactiveProperty();

	private readonly SerialDisposable m_InstallBindDisposable = new SerialDisposable();

	public readonly BoolReactiveProperty IsRightWindow = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsLeftFocus = new BoolReactiveProperty();

	private IItemSlotView m_CurrentEntity;

	[SerializeField]
	protected FlexibleLensSelectorView m_SelectorView;

	public DollRoomTargetController Controller => m_CharacterController;

	public override void Initialize()
	{
		base.Initialize();
		InventorySelectorWindowConsoleView selectorWindowView = m_SelectorWindowView;
		if ((object)selectorWindowView != null)
		{
			selectorWindowView.Or(null).Initialize();
		}
		m_StashView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(m_InstallBindDisposable);
		m_StashView.Bind(base.ViewModel.StashVM);
		base.BindViewImplementation();
		UpdateSlots();
		UpdateNavigation();
		AddDisposable(m_NavigationBehaviour?.DeepestFocusAsObservable.Subscribe(OnFocusEntity));
		AddDisposable(base.ViewModel.Unit?.Subscribe(delegate
		{
			RefreshView();
		}));
		if (base.ViewModel.StarSystemSpaceBarksHolderVM != null)
		{
			m_StarSystemSpaceBarksHolderConsoleView.Bind(base.ViewModel.StarSystemSpaceBarksHolderVM);
		}
		if ((bool)m_SelectorWindowView)
		{
			AddDisposable(base.ViewModel.InventorySelectorWindowVM.Subscribe(m_SelectorWindowView.Bind));
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		EventBus.RaiseEvent(delegate(IPartySelectorAugmentationHintsAndInputHandler h)
		{
			h.DisposeInputImpl();
		});
	}

	private void UpdateNavigation()
	{
		m_InstallBindDisposable.Disposable = null;
		if (m_NavigationBehaviour == null)
		{
			AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
			AddDisposable(m_SlotsNavigation = new FloatConsoleNavigationBehaviour(m_Parameters));
		}
		else
		{
			m_NavigationBehaviour.Clear();
		}
		m_AugmentationsInputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Augmentations Console View"
		});
		InputBindStruct inputBindStruct = m_AugmentationsInputLayer.AddButton(delegate
		{
			Close();
		}, 9);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.CloseWindow, ConsoleHintsWidget.HintPosition.Right));
		AddDisposable(inputBindStruct);
		InputBindStruct inputBindStruct2 = m_AugmentationsInputLayer.AddButton(delegate
		{
			OnConfirm();
		}, 8, m_CanEquip);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.ContextMenu.Equip, ConsoleHintsWidget.HintPosition.Right));
		AddDisposable(inputBindStruct2);
		InputBindStruct inputBindStruct3 = m_AugmentationsInputLayer.AddButton(delegate
		{
			OnConfirm();
		}, 8, m_CanChoose);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.CommonTexts.Select, ConsoleHintsWidget.HintPosition.Right));
		AddDisposable(inputBindStruct3);
		InputBindStruct inputBindStruct4 = m_AugmentationsInputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct4, UIStrings.Instance.CommonTexts.Information, ConsoleHintsWidget.HintPosition.Right));
		AddDisposable(inputBindStruct4);
		InputBindStruct inputBindStruct5 = m_AugmentationsInputLayer.AddButton(delegate
		{
			RotateToDefaultPosition();
		}, 18);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct5, UIStrings.Instance.ShipCustomization.ToDefaultPosition, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(inputBindStruct5);
		InputBindStruct inputBindStruct6 = m_AugmentationsInputLayer.AddLongPressButton(delegate
		{
			AugmentsInfoShowHandler();
		}, 19, InputActionEventType.ButtonLongPressed);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct6, UIStrings.Instance.UIAugmentations.InspectAllAugments, ConsoleHintsWidget.HintPosition.Left));
		AddDisposable(inputBindStruct6);
		InputBindStruct inputBindStruct7 = m_AugmentationsInputLayer.AddButton(ShowContextMenu, 10, m_HasContextMenu, InputActionEventType.ButtonShortPressJustReleased);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct7, UIStrings.Instance.ContextMenu.ContextMenu, ConsoleHintsWidget.HintPosition.Right));
		AddDisposable(inputBindStruct7);
		InputBindStruct inputBindStruct8 = m_AugmentationsInputLayer.AddButton(delegate
		{
			ToggleCurrentSlotOverride();
		}, 11, m_HasOverdrive, InputActionEventType.ButtonJustReleased);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct8, UIStrings.Instance.UIAugmentations.ConsoleHintOverdrive, ConsoleHintsWidget.HintPosition.Right));
		AddDisposable(inputBindStruct8);
		AddDisposable(m_AugmentationsInputLayer.AddAxis(RotateDoll, 2));
		m_NavigationBehaviour.AddColumn<ConsoleNavigationBehaviour>(GetNavigation());
		m_StashView.ItemsFilter.AddInput(m_AugmentationsInputLayer, IsRightWindow);
		if (m_IsCulled)
		{
			m_NavigationBehaviour.UnFocusCurrentEntity();
			return;
		}
		m_NavigationBehaviour.FocusOnEntityManual(m_StashView.SlotsNavigation);
		AddDisposable(GamePad.Instance.PushLayer(m_AugmentationsInputLayer));
		EventBus.RaiseEvent(delegate(IPartySelectorAugmentationHintsAndInputHandler h)
		{
			h.DisposeInputImpl();
		});
		EventBus.RaiseEvent(delegate(IPartySelectorAugmentationHintsAndInputHandler h)
		{
			h.CreateInputImpl(m_AugmentationsInputLayer, IsLeftFocus);
		});
	}

	public void HandleRemoveFocus()
	{
		m_IsCulled = true;
		m_CulledFocus = m_NavigationBehaviour?.DeepestNestedFocus;
		m_NavigationBehaviour?.UnFocusCurrentEntity();
	}

	public void HandleRestoreFocus()
	{
		m_IsCulled = false;
		if (m_NavigationBehaviour == null)
		{
			m_CulledFocus = null;
			return;
		}
		UpdateNavigation();
		if (m_CulledFocus != null)
		{
			m_NavigationBehaviour.FocusOnEntityManual(m_CulledFocus);
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
		}
		m_CulledFocus = null;
	}

	private void ProcessAugmentInstall()
	{
		if (m_CurrentEntity is AugmentationsEquipSlotConsoleView augmentationsEquipSlotConsoleView)
		{
			augmentationsEquipSlotConsoleView.OnInstallButtonConsole();
		}
	}

	private void ToggleCurrentSlotOverride()
	{
		if (m_CurrentEntity is AugmentationsEquipSlotConsoleView augmentationsEquipSlotConsoleView)
		{
			augmentationsEquipSlotConsoleView.TryOverdrive();
		}
	}

	private void RotateToDefaultPosition()
	{
		UIDollRooms.Instance.AugmentationsDollRoom.RotateToDefaultPosition();
	}

	private ConsoleNavigationBehaviour GetNavigation()
	{
		List<IFloatConsoleNavigationEntity> list = new List<IFloatConsoleNavigationEntity>();
		if (SlotIsBinded(m_MechSlot1Pascal))
		{
			list.Add(m_MechSlot1Pascal.GetOverchargeAbilitySlot());
			list.Add(m_MechSlot1Pascal);
		}
		if (SlotIsBinded(m_MechSlot1Manipulus))
		{
			list.Add(m_MechSlot1Manipulus.GetOverchargeAbilitySlot());
			list.Add(m_MechSlot1Manipulus);
		}
		if (SlotIsBinded(m_ForgeWorld))
		{
			list.Add(m_ForgeWorld);
			list.Add(m_ForgeWorld.GetOverchargeAbilitySlot());
		}
		list.Add(m_NervousSystem);
		list.Add(m_NervousSystem.GetOverchargeAbilitySlot());
		List<IFloatConsoleNavigationEntity> entities = new List<IFloatConsoleNavigationEntity>
		{
			m_InternalSystems.GetOverchargeAbilitySlot(),
			m_InternalSystems,
			m_PreceptionSystem,
			m_PreceptionSystem.GetOverchargeAbilitySlot()
		};
		List<IFloatConsoleNavigationEntity> list2 = new List<IFloatConsoleNavigationEntity>();
		if (SlotIsBinded(m_MechSlot3Pascal))
		{
			list2.Add(m_MechSlot3Pascal.GetOverchargeAbilitySlot());
			list2.Add(m_MechSlot3Pascal);
		}
		if (SlotIsBinded(m_MechSlot3Manipulus))
		{
			list2.Add(m_MechSlot3Manipulus.GetOverchargeAbilitySlot());
			list2.Add(m_MechSlot3Manipulus);
		}
		if (SlotIsBinded(m_MechSlot2Pascal))
		{
			list2.Add(m_MechSlot2Pascal);
			list2.Add(m_MechSlot2Pascal.GetOverchargeAbilitySlot());
		}
		if (SlotIsBinded(m_MechSlot2Manipulus))
		{
			list2.Add(m_MechSlot2Manipulus);
			list2.Add(m_MechSlot2Manipulus.GetOverchargeAbilitySlot());
		}
		List<IFloatConsoleNavigationEntity> entities2 = new List<IFloatConsoleNavigationEntity>
		{
			m_RightHand.GetOverchargeAbilitySlot(),
			m_RightHand,
			m_LeftHand,
			m_LeftHand.GetOverchargeAbilitySlot()
		};
		List<IFloatConsoleNavigationEntity> entities3 = new List<IFloatConsoleNavigationEntity>
		{
			m_Legs,
			m_Legs.GetOverchargeAbilitySlot()
		};
		List<IFloatConsoleNavigationEntity> entities4 = new List<IFloatConsoleNavigationEntity>
		{
			m_OverchargeSlot,
			m_OverchargeSlot.GetOverchargeAbilitySlot()
		};
		FloatConsoleNavigationBehaviour floatConsoleNavigationBehaviour = new FloatConsoleNavigationBehaviour(m_Parameters);
		floatConsoleNavigationBehaviour.AddEntities(list);
		floatConsoleNavigationBehaviour.AddEntities(entities);
		if (list2.Count > 0)
		{
			floatConsoleNavigationBehaviour.AddEntities(list2);
		}
		floatConsoleNavigationBehaviour.AddEntities(entities2);
		floatConsoleNavigationBehaviour.AddEntities(entities3);
		floatConsoleNavigationBehaviour.AddEntities(entities4);
		ConsoleNavigationBehaviour navigation = m_StashView.GetNavigation();
		m_NavigationBehaviour.AddEntityGrid(floatConsoleNavigationBehaviour);
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		gridConsoleNavigationBehaviour.AddEntityGrid(navigation);
		m_ChooseSlotInputLayer = m_SlotsNavigation.GetInputLayer(new InputLayer
		{
			ContextName = "ChooseSlot"
		});
		gridConsoleNavigationBehaviour.FocusOnEntityManual(navigation);
		CreateChooseSlotNavigation();
		return gridConsoleNavigationBehaviour;
	}

	private void CreateChooseSlotNavigation()
	{
		AddDisposable(base.ViewModel.ChooseSlotMode.Skip(1).Subscribe(delegate(bool on)
		{
			if (on)
			{
				PushChooseSlotNavigation();
			}
			else
			{
				PopChooseSlotNavigation();
			}
		}));
		if ((bool)m_ConsoleHintsWidget)
		{
			AddDisposable(m_ConsoleHintsWidget.BindHint(m_ChooseSlotInputLayer.AddButton(delegate
			{
				base.ViewModel.ChooseSlotMode.Value = false;
			}, 9, base.ViewModel.ChooseSlotMode), UIStrings.Instance.CommonTexts.Cancel));
			AddDisposable(m_ConsoleHintsWidget.BindHint(m_ChooseSlotInputLayer.AddButton(delegate
			{
			}, 8, base.ViewModel.ChooseSlotMode), UIStrings.Instance.CommonTexts.Select));
		}
	}

	private void PushChooseSlotNavigation()
	{
		m_SlotsNavigation.Clear();
		foreach (var (augmentationsSlotVM2, entities) in m_SlotsMap)
		{
			if (augmentationsSlotVM2.IsPossibleTarget(base.ViewModel.ItemToSlotView.Item))
			{
				m_SlotsNavigation.AddEntity(entities);
				augmentationsSlotVM2.SetPossibleTargetState(state: false);
			}
		}
		AddDisposable(GamePad.Instance.PushLayer(m_ChooseSlotInputLayer));
		m_SlotsNavigation.FocusOnFirstValidEntity();
	}

	private void PopChooseSlotNavigation()
	{
		GamePad.Instance.PopLayer(m_ChooseSlotInputLayer);
		m_SlotsNavigation.Clear();
		foreach (KeyValuePair<AugmentationsSlotVM, AugmentationsEquipSlotConsoleView> item in m_SlotsMap)
		{
			item.Deconstruct(out var key, out var _);
			key.SetPossibleTargetState(state: true);
		}
		base.ViewModel.ItemToSlotView.ReleaseSlot();
		Refocus();
		OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void RefreshView()
	{
		if (!(base.AugmentationsDollRoom == null) && base.ViewModel.Unit.Value != null)
		{
			IConsoleEntity consoleEntity = m_NavigationBehaviour?.DeepestNestedFocus;
			bool flag = consoleEntity != null && !(consoleEntity is InventorySlotConsoleView);
			try
			{
				base.AugmentationsDollRoom.SetupUnit(base.ViewModel.Unit.Value);
			}
			catch (Exception ex)
			{
				PFLog.UI.Exception(ex);
			}
			UpdateSlots();
			UpdateNavigation();
			if (flag && !m_IsCulled && m_NavigationBehaviour != null)
			{
				m_NavigationBehaviour.FocusOnEntityManual(consoleEntity);
				m_NavigationBehaviour.UpdateDeepestFocusObserve();
			}
		}
	}

	private void RotateDoll(InputActionEventData obj, float x)
	{
		base.ViewModel.HideLines();
		m_CharacterController.Rotate((0f - x) * m_RotateFactor);
	}

	public void SetCanvasScaler(CanvasScalerWorkaround canvasScaler)
	{
		m_CharacterController.CanvasScaler = canvasScaler;
	}

	private void UpdateSlots()
	{
		BindSlot(m_NervousSystem);
		BindSlot(m_PreceptionSystem);
		BindSlot(m_RightHand);
		BindSlot(m_LeftHand);
		BindSlot(m_InternalSystems);
		BindSlot(m_Legs);
		BindSlot(m_MechSlot1Pascal);
		BindSlot(m_MechSlot2Pascal);
		BindSlot(m_MechSlot3Pascal);
		BindSlot(m_MechSlot1Manipulus);
		BindSlot(m_MechSlot2Manipulus);
		BindSlot(m_MechSlot3Manipulus);
		BindSlot(m_ForgeWorld);
		BindOverrideSlot(m_OverchargeSlot);
	}

	private void BindSlot(AugmentationsEquipSlotConsoleView slot)
	{
		AugmentationsSlotVM augmentationsSlotVM = Enumerable.FirstOrDefault(base.ViewModel.AllAugmentSlots, (AugmentationsSlotVM b) => b.BlueprintAugmentSlot == slot.SlotTypeReference.Get() && !b.IsOverchargeSlot);
		slot.gameObject.SetActive(augmentationsSlotVM != null);
		if (augmentationsSlotVM != null && !slot.IsBinded)
		{
			slot.Bind(augmentationsSlotVM);
			m_SlotsMap.TryAdd(augmentationsSlotVM, slot);
			AddDisposable(ObservableExtensions.Subscribe(augmentationsSlotVM.RefreshOverchargeAbility, delegate
			{
				DeepestNestedFocus();
			}));
		}
	}

	private bool SlotIsBinded(AugmentationsEquipSlotConsoleView slot)
	{
		return base.ViewModel.AllAugmentSlots.FirstOrDefault((AugmentationsSlotVM s) => s == slot.GetViewModel()) != null;
	}

	private void BindOverrideSlot(AugmentationsEquipSlotConsoleView slot)
	{
		UnitAugments augments = base.ViewModel.UnitAugments;
		slot.MarkAsOverchargeSlotView();
		AugmentationsSlotVM augmentationsSlotVM = Enumerable.FirstOrDefault(base.ViewModel.AllAugmentSlots, (AugmentationsSlotVM b) => b.BlueprintAugmentSlot == augments.OverdriveSlot);
		if (augmentationsSlotVM == null)
		{
			AugmentationsSlotVM augmentationsSlotVM2 = Enumerable.FirstOrDefault(base.ViewModel.AllAugmentSlots, (AugmentationsSlotVM b) => b.IsOverchargeSlot);
			slot.Bind(augmentationsSlotVM2);
			augmentationsSlotVM2?.RefreshOverchargeAbility.Execute();
		}
		else
		{
			slot.Bind(augmentationsSlotVM);
			augmentationsSlotVM.RefreshOverchargeAbility.Execute();
		}
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		m_CanEquip.Value = entity is InventorySlotConsoleView inventorySlotConsoleView && inventorySlotConsoleView.SlotVM.CanUse.Value;
		if (entity is AugmentationsEquipSlotConsoleView augmentationsEquipSlotConsoleView && augmentationsEquipSlotConsoleView.GetViewModel() is AugmentationsSlotVM augmentationsSlotVM && augmentationsSlotVM.IsDirty.Value)
		{
			m_InstallButton.Value = true;
			InputBindStruct inputBindStruct = m_AugmentationsInputLayer.AddButton(delegate
			{
				ProcessAugmentInstall();
			}, 10, m_InstallButton, InputActionEventType.ButtonJustLongPressed);
			IDisposable disposable = augmentationsEquipSlotConsoleView.SetConsoleHint(inputBindStruct);
			m_InstallBindDisposable.Disposable = new CompositeDisposable(inputBindStruct, disposable);
		}
		else
		{
			m_InstallButton.Value = false;
			m_InstallBindDisposable.Disposable = null;
		}
		HandleTooltip(entity);
		m_CurrentEntity = entity as IItemSlotView;
		IItemSlotView currentEntity = m_CurrentEntity;
		int num;
		if (currentEntity != null)
		{
			ItemSlotVM slotVM = currentEntity.SlotVM;
			if (slotVM != null && slotVM.HasItem)
			{
				num = ((m_CurrentEntity.SlotVM?.ContextMenu?.Value.Any((ContextMenuCollectionEntity item) => item.IsEnabled)).GetValueOrDefault() ? 1 : 0);
				goto IL_0160;
			}
		}
		num = 0;
		goto IL_0160;
		IL_0160:
		bool value = (byte)num != 0;
		m_HasContextMenu.Value = value;
		m_CanChoose.Value = entity is AugmentationsEquipSlotConsoleView { SlotVM: AugmentationsSlotVM slotVM2 } && !slotVM2.IsOverchargeSlot;
		m_HasOverdrive.Value = entity is AugmentationsEquipSlotConsoleView { SlotVM: AugmentationsSlotVM { IsDirty: { Value: false }, ItemSlot: AugmentSlot { ItemBlueprint: { OverdriveAbility: not null } } } slotVM3 } && !slotVM3.IsOverchargeSlot;
		IsRightWindow.Value = entity is InventorySlotConsoleView;
		IsLeftFocus.Value = !(entity is InventorySlotConsoleView);
	}

	private void Close()
	{
		TooltipHelper.HideTooltip();
		base.ViewModel.RemoveNotInstalled();
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
	}

	private void OnConfirm()
	{
		DelayedInvoker.InvokeInFrames(DeepestNestedFocus, 10);
	}

	private void HandleTooltip(IConsoleEntity entity)
	{
		TooltipHelper.HideTooltip();
		if (entity == null)
		{
			m_HasTooltip.Value = false;
			return;
		}
		MonoBehaviour monoBehaviour = (entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour;
		if (monoBehaviour == null)
		{
			m_HasTooltip.Value = false;
			return;
		}
		if (entity is SimpleConsoleNavigationEntity simpleConsoleNavigationEntity)
		{
			m_HasTooltip.Value = simpleConsoleNavigationEntity.TooltipTemplate() != null;
			if (m_ShowTooltip.Value)
			{
				MonoBehaviour tooltipPlace = simpleConsoleNavigationEntity.GetTooltipPlace();
				if ((bool)tooltipPlace)
				{
					tooltipPlace.ShowConsoleTooltip(simpleConsoleNavigationEntity.TooltipTemplate(), m_NavigationBehaviour);
				}
				else
				{
					monoBehaviour.ShowConsoleTooltip(simpleConsoleNavigationEntity.TooltipTemplate(), m_NavigationBehaviour);
				}
			}
			return;
		}
		UpdateTooltipConfigs();
		if (entity is IHasTooltipTemplate hasTooltipTemplate)
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
			m_HasTooltip.Value = list != null && list.Count > 0;
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
		if ((bool)m_StashTooltipPlaces)
		{
			m_MainTooltipConfig = m_StashTooltipPlaces.GetMainTooltipConfig(m_MainTooltipConfig);
			m_CompareTooltipConfig = m_StashTooltipPlaces.GetCompareTooltipConfig(m_CompareTooltipConfig);
		}
		else
		{
			m_MainTooltipConfig.PriorityPivots = new List<Vector2>
			{
				new Vector2(0.5f, 0.5f)
			};
		}
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip.Value = !m_ShowTooltip.Value;
		OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void ShowContextMenu(InputActionEventData obj)
	{
		if (m_CurrentEntity is IConsoleEntity consoleEntity)
		{
			TooltipHelper.HideTooltip();
			((consoleEntity as MonoBehaviour) ?? (consoleEntity as IMonoBehaviour)?.MonoBehaviour).ShowContextMenu(m_CurrentEntity.SlotVM?.ContextMenu?.Value);
		}
	}

	private void DeepestNestedFocus()
	{
		if (m_NavigationBehaviour == null)
		{
			UpdateNavigation();
		}
		OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void Refocus()
	{
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	public void HandleForceRebindOverdriveBlock()
	{
		BindOverrideSlot(m_OverchargeSlot);
	}

	public void HandleAugmentActivateOverdrive(BaseUnitEntity owner)
	{
		if (base.ViewModel.Unit.Value == owner)
		{
			BindOverrideSlot(m_OverchargeSlot);
		}
	}

	public void HandleAugmentDeactivateOverdrive(BaseUnitEntity owner)
	{
		if (base.ViewModel.Unit.Value == owner)
		{
			BindOverrideSlot(m_OverchargeSlot);
		}
	}

	public void AugmentsInfoShowHandler()
	{
		EventBus.RaiseEvent(delegate(IUnitClickUIHandler h)
		{
			h.HandleUnitConsoleInvoke(base.ViewModel.Unit.Value);
		});
	}
}
