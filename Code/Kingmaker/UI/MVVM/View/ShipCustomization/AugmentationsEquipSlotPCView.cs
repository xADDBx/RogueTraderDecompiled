using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ActionBar;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class AugmentationsEquipSlotPCView : AugmentationsEquipSlotBaseView<ItemSlotPCView>
{
	[SerializeField]
	protected SurfaceActionBarSlotAbilityView m_OverchargeAbility;

	[SerializeField]
	private GameObject m_OverchargeIconNo;

	protected override void BindViewImplementation()
	{
		m_ItemSlotView.IsDraggable = !base.ViewModel.IsLocked.Value;
		m_ItemSlotView.Bind(base.ViewModel);
		base.BindViewImplementation();
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(m_ItemSlotView.OnSingleLeftClickAsObservable.Subscribe(base.OnClick));
		AddDisposable(m_ItemSlotView.OnPointerEnterAsObservable().Subscribe(delegate
		{
			OnHoverStart();
		}));
		AddDisposable(m_ItemSlotView.OnPointerExitAsObservable().Subscribe(delegate
		{
			OnHoverEnd();
		}));
		if (m_IsOverchargeSlotView)
		{
			AddDisposable(m_ItemSlotView.OnBeginDragCommand.Subscribe(OnOverchargeBeginDrag));
			AddDisposable(m_ItemSlotView.OnEndDragCommand.Subscribe(OnOverchargeEndDrag));
		}
		else
		{
			AddDisposable(m_ItemSlotView.OnBeginDragCommand.Subscribe(OnBeginDrag));
			AddDisposable(m_ItemSlotView.OnEndDragCommand.Subscribe(OnEndDrag));
		}
		AddDisposable(base.ViewModel.Item.Subscribe(delegate(ItemEntity value)
		{
			m_EmptyPlaceholder.SetActive(value == null);
		}));
		AddDisposable(m_EmptyPlaceholderButton.SetHint(SetupHint(base.ViewModel.SlotType)));
		AddDisposable(UniRxExtensionMethods.Subscribe(base.ViewModel.RefreshOverchargeAbility, delegate
		{
			SetOverchargeAbilitySlot();
			SetOverchargeButtonState();
		}));
		AddDisposable(m_OverchargeButton.OnPointerClickAsObservable().Subscribe(delegate
		{
			OnOverrideButtonClick();
		}));
		if (m_InstallButton != null)
		{
			AddDisposable(m_InstallButton.OnPointerClickAsObservable().Subscribe(delegate
			{
				OnInstallButtonClick();
			}));
		}
		if (m_PossibleTargetHighlight != null)
		{
			AddDisposable(base.ViewModel.PossibleTarget.Subscribe(m_PossibleTargetHighlight.SetActive));
		}
		AddDisposable(base.ViewModel.PlayerSize.Subscribe(delegate(AugmentationsPlayerSize value)
		{
			SetLinesType(value);
		}));
		AddDisposable(base.ViewModel.ToggleLinesCommand.Subscribe(delegate(bool value)
		{
			ToggleLines(value);
		}));
		SetSlotState(isActive: true);
		SetOverchargeButtonState();
		SetInstallButtonInitialState();
		SetOverchargeAbilitySlot();
		if ((bool)m_LinesBlock)
		{
			m_LinesBlock.alpha = 1f;
			m_isOverchargeSlot = base.ViewModel.IsOverchargeSlot;
			UISounds.Instance.SetClickSound(m_InstallButton, UISounds.ButtonSoundsEnum.NoSound);
		}
	}

	public void SetOverchargeAbilitySlot()
	{
		if (!(base.ViewModel.ItemSlot is AugmentSlot augmentSlot))
		{
			ActionBarSlotVM viewModel = new ActionBarSlotVM(new MechanicActionBarSlotEmpty
			{
				Unit = base.ViewModel.Unit
			});
			m_OverchargeAbility.Initialize();
			m_OverchargeAbility.Bind(viewModel);
			m_OverchargeAbility.SetGreyscale(value: true);
			return;
		}
		if (augmentSlot.ItemBlueprint == null)
		{
			if (base.ViewModel.IsOverchargeSlot)
			{
				ActionBarSlotVM viewModel2 = new ActionBarSlotVM(new MechanicActionBarSlotEmpty
				{
					Unit = base.ViewModel.Unit
				});
				m_OverchargeAbility.Initialize();
				m_OverchargeAbility.Bind(viewModel2);
				m_OverchargeAbility.SetGreyscale(value: true);
			}
			else
			{
				m_OverchargeAbility.gameObject.SetActive(value: false);
			}
			return;
		}
		BlueprintAbility overdriveAbility = augmentSlot.ItemBlueprint.OverdriveAbility;
		if (overdriveAbility == null)
		{
			if (base.ViewModel.IsOverchargeSlot)
			{
				ActionBarSlotVM viewModel3 = new ActionBarSlotVM(new MechanicActionBarSlotEmpty
				{
					Unit = base.ViewModel.Unit
				});
				m_OverchargeAbility.Bind(viewModel3);
				m_OverchargeAbility.SetGreyscale(value: true);
			}
			else
			{
				m_OverchargeAbility.gameObject.SetActive(value: false);
			}
			return;
		}
		Ability ability;
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			ability = new Ability(overdriveAbility, base.ViewModel.Unit);
		}
		ActionBarSlotVM actionBarSlotVM = new ActionBarSlotVM(new MechanicActionBarSlotAugmentsOverchargeAbility(ability, base.ViewModel.Unit), -1, isInCharScreen: true);
		AddDisposable(actionBarSlotVM);
		m_OverchargeAbility.Initialize();
		m_OverchargeAbility.Bind(actionBarSlotVM);
		m_OverchargeAbility.SetGreyscale(value: true);
	}

	private void SetOverchargeButtonState()
	{
		ItemSlot itemSlot = base.ViewModel.ItemSlot;
		if (itemSlot != null && !itemSlot.HasItem)
		{
			if (m_OverchargeButton != null)
			{
				m_OverchargeButton.gameObject.SetActive(value: false);
			}
			return;
		}
		AugmentationsSlotVM viewModel = base.ViewModel;
		if (viewModel == null || !(viewModel.ItemSlot is AugmentSlot augmentSlot))
		{
			if (!(m_OverchargeButton == null))
			{
				m_OverchargeButton.gameObject.SetActive(value: false);
			}
			return;
		}
		bool flag = augmentSlot.ItemBlueprint.OverdriveAbility != null && !base.ViewModel.IsDirty.Value;
		bool flag2 = base.ViewModel.Unit.GetOptional<PartUnitBody>()?.Augments.OverdriveSlot == base.ViewModel.BlueprintAugmentSlot;
		if (!(m_OverchargeButton == null))
		{
			m_OverchargeButton.gameObject.SetActive(flag);
			m_OverchargeButton.SetActiveLayer(0);
			if (flag && !Game.Instance.TurnController.InCombat)
			{
				m_OverchargeButton.SetActiveLayer(1);
			}
			if (flag2)
			{
				m_OverchargeButton.SetActiveLayer(2);
			}
			if (Game.Instance.TurnController.InCombat)
			{
				m_OverchargeButton.Interactable = flag && flag2;
			}
			else
			{
				m_OverchargeButton.Interactable = flag;
			}
			m_OverchargeIconNo.SetActive(Game.Instance.TurnController.InCombat && m_OverchargeButton.ActiveLayerIndex == 2);
			if (Game.Instance.TurnController.InCombat)
			{
				m_OverchargeButton.SetHint(UIStrings.Instance.UIAugmentations.CannotUseInCombat);
			}
			else if (m_OverchargeButton.ActiveLayerIndex == 1)
			{
				m_OverchargeButton.SetHint(UIStrings.Instance.UIAugmentations.OverdriveThisHint);
			}
		}
	}

	private void OnOverrideButtonClick()
	{
		if (!Game.Instance.TurnController.InCombat && m_OverchargeButton.Interactable)
		{
			int currentLayer = m_OverchargeButton.ActiveLayerIndex;
			EventBus.RaiseEvent(delegate(IAugmentOverdriveToggleHandler h)
			{
				h.HandleAugmentOverdriveToggle(base.ViewModel.BlueprintAugmentSlot, currentLayer);
			});
			switch (currentLayer)
			{
			case 1:
				m_OverchargeButton.SetActiveLayer(2);
				m_OverchargeButton.SetHint(UIStrings.Instance.UIAugmentations.OverdriveThisHint, null, default(Color), shouldShow: false);
				break;
			case 2:
				m_OverchargeButton.SetActiveLayer(1);
				m_OverchargeButton.SetHint(UIStrings.Instance.UIAugmentations.OverdriveThisHint);
				break;
			}
		}
	}

	protected override void DestroyViewImplementation()
	{
		SetSlotState(m_isOverchargeSlot);
	}

	public void HandleOnRotationStart()
	{
		if ((bool)m_LinesBlock)
		{
			m_LinesBlock.alpha = 0f;
		}
	}

	public void HandleOnRotationStop()
	{
		if ((bool)m_LinesBlock)
		{
			m_LinesBlock.alpha = 1f;
		}
	}

	private void OnBeginDrag()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotPossibleTarget h)
		{
			h.HandleHighlightStart(base.Item);
		});
	}

	private void OnEndDrag()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotPossibleTarget h)
		{
			h.HandleHighlightStop();
		});
	}

	private void OnOverchargeBeginDrag()
	{
		EventBus.RaiseEvent(delegate(IAugmentOverchargeSlotDragHandler h)
		{
			h.HandleOverchargeSlotDragStart();
		});
	}

	private void OnOverchargeEndDrag()
	{
		EventBus.RaiseEvent(delegate(IAugmentOverchargeSlotDragHandler h)
		{
			h.HandleOverchargeSlotDragEnd();
		});
	}
}
