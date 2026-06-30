using System.Collections.Generic;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class AugmentationsEquipSlotBaseView<TItemSlotView> : ItemSlotView<AugmentationsSlotVM>, IHasTooltipTemplates, INewSlotsHandler, ISubscriber where TItemSlotView : ItemSlotBaseView
{
	[Header("Common")]
	[SerializeField]
	private CanvasGroup m_SlotCanvasGroup;

	[FormerlySerializedAs("ItemSlotView")]
	[FormerlySerializedAs("m_ItemSlotPCView")]
	[SerializeField]
	protected TItemSlotView m_ItemSlotView;

	[SerializeField]
	protected OwlcatMultiButton m_LockState;

	[SerializeField]
	protected GameObject m_EmptyPlaceholder;

	[SerializeField]
	protected OwlcatButton m_EmptyPlaceholderButton;

	[SerializeField]
	protected GameObject m_PossibleTargetHighlight;

	[SerializeField]
	private TextMeshProUGUI m_ShortDescription;

	[SerializeField]
	protected CanvasGroup m_LinesBlock;

	[SerializeField]
	private GameObject[] m_ShipLines;

	[SerializeField]
	protected OwlcatMultiButton m_OverchargeButton;

	[SerializeField]
	protected OwlcatMultiButton m_InstallButton;

	[SerializeField]
	private TextMeshProUGUI m_InstallButtonText;

	[SerializeField]
	protected FadeAnimator m_InstallButtonFade;

	public BlueprintAugmentSlotReference SlotTypeReference;

	protected bool m_isOverchargeSlot;

	protected bool m_IsOverchargeSlotView;

	public void MarkAsOverchargeSlotView()
	{
		m_IsOverchargeSlotView = true;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.IsLocked.Subscribe(delegate
		{
			RefreshLockState();
		}));
		AddDisposable(base.ViewModel.Item.Subscribe(delegate
		{
			RefreshLockState();
		}));
		AddDisposable(base.ViewModel.RefreshOverchargeAbility.Subscribe(delegate
		{
			RefreshLockState();
		}));
		if (m_InstallButtonFade != null)
		{
			AddDisposable(base.ViewModel.IsDirty.Subscribe(delegate(bool value)
			{
				if (value)
				{
					m_InstallButtonFade.AppearAnimation();
				}
				else
				{
					m_InstallButtonFade.DisappearAnimation();
				}
			}));
		}
		if (m_InstallButtonText != null)
		{
			m_InstallButtonText.text = UIStrings.Instance.UIAugmentations.InstallButtonText;
		}
	}

	protected override void SetupContextMenu()
	{
		base.SetupContextMenu();
		if (!m_IsOverchargeSlotView)
		{
			UIContextMenu contextMenu = UIStrings.Instance.ContextMenu;
			bool isInteractable = CanTakeOffAugment() && base.ViewModel.IsEquipPossible;
			base.ViewModel.ContextMenu.Value = new List<ContextMenuCollectionEntity>
			{
				new ContextMenuCollectionEntity(contextMenu.TakeOff, delegate
				{
					TryUnequip();
				}, base.ViewModel.HasItem, isInteractable),
				new ContextMenuCollectionEntity(contextMenu.Information, base.ViewModel.ShowInfo, base.ViewModel.HasItem)
			};
		}
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return base.ViewModel.Tooltip.Value;
	}

	protected void SetLinesType(AugmentationsPlayerSize value)
	{
		if (m_ShipLines.Length != 0)
		{
			GameObject[] shipLines = m_ShipLines;
			for (int i = 0; i < shipLines.Length; i++)
			{
				shipLines[i].SetActive(value: false);
			}
			switch (value)
			{
			case AugmentationsPlayerSize.Regular:
				m_ShipLines[0].SetActive(value: true);
				break;
			case AugmentationsPlayerSize.Mech:
				m_ShipLines[1].SetActive(value: true);
				break;
			case AugmentationsPlayerSize.Big:
				m_ShipLines[2].SetActive(value: true);
				break;
			}
		}
	}

	protected void ToggleLines(bool value)
	{
		if (value)
		{
			SetLinesType(base.ViewModel.PlayerSize.Value);
		}
		else if (m_ShipLines.Length != 0)
		{
			GameObject[] shipLines = m_ShipLines;
			for (int i = 0; i < shipLines.Length; i++)
			{
				shipLines[i].SetActive(value: false);
			}
		}
	}

	protected void SetSlotState(bool isActive)
	{
		m_SlotCanvasGroup.interactable = isActive;
		m_SlotCanvasGroup.alpha = (isActive ? 1f : 0.3f);
		isActive = base.ViewModel.IsOverchargeSlot || isActive;
		m_ItemSlotView.gameObject.SetActive(isActive);
	}

	protected void SetInstallButtonInitialState()
	{
		if (!(m_InstallButton == null) && !(m_InstallButtonFade == null))
		{
			m_InstallButton.SetActiveLayer(0);
			m_InstallButtonFade.DisappearAnimation();
		}
	}

	protected void OnHoverStart()
	{
	}

	protected void OnHoverEnd()
	{
		EventBus.RaiseEvent(delegate(IInventorySlotHoverHandler h)
		{
			h.HandleHoverStop();
		});
	}

	protected void OnClick()
	{
		if (!m_IsOverchargeSlotView && !base.ViewModel.IsLocked.Value)
		{
			EventBus.RaiseEvent(delegate(IAugmentationItemHandler h)
			{
				h.HandleChangeItem(base.ViewModel);
			});
		}
	}

	public void HandleTryInsertSlot(InsertableLootSlotVM slot)
	{
	}

	public void HandleTryMoveSlot(ItemSlotVM from, ItemSlotVM to)
	{
	}

	public void HandleTrySplitSlot(ItemSlotVM slot)
	{
		if (base.ViewModel.IsLocked.Value)
		{
			UISounds.Instance.Sounds.AugmentationsWindow.AugmentViewOnlyInstall.Play();
		}
		else
		{
			InventoryHelper.TrySplitSlot(slot, isLoot: false);
		}
	}

	protected bool CanTakeOffAugment()
	{
		if (!base.ViewModel.HasItem)
		{
			return false;
		}
		if (base.ViewModel.IsDefault)
		{
			return false;
		}
		if (base.ViewModel.BlueprintAugmentSlot != null && base.ViewModel.BlueprintAugmentSlot.IsMechSlot)
		{
			return false;
		}
		if (base.ViewModel.IsLocked.Value)
		{
			return false;
		}
		if (base.ViewModel.HasTrauma.Value)
		{
			return false;
		}
		if (base.ViewModel.ItemSlot == null || !base.ViewModel.ItemSlot.CanRemoveItem())
		{
			return false;
		}
		return true;
	}

	private bool TryUnequip()
	{
		if (base.ViewModel.ItemSlot.HasItem && base.ViewModel.BlueprintAugmentSlot.DefaultAugment != null && base.ViewModel.ItemSlot.Item.Blueprint == base.ViewModel.BlueprintAugmentSlot.DefaultAugment.Get())
		{
			return false;
		}
		if (base.ViewModel.BlueprintAugmentSlot.IsMechSlot)
		{
			UISounds.Instance.Sounds.AugmentationsWindow.AugmentMechRestriction.Play();
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.UIAugmentations.CannotRemoveMechAugment.Text, addToLog: false, WarningNotificationFormat.Short);
			});
			return false;
		}
		if (base.ViewModel.IsLocked.Value)
		{
			UISounds.Instance.Sounds.AugmentationsWindow.AugmentViewOnlyInstall.Play();
			return false;
		}
		if (base.ViewModel.HasTrauma.Value)
		{
			UISounds.Instance.Sounds.AugmentationsWindow.AugmentBrokenInstall.Play();
			return false;
		}
		bool flag = base.ViewModel.BlueprintAugmentSlot != null && base.ViewModel.Unit?.GetOptional<PartUnitBody>()?.Augments.OverdriveSlot == base.ViewModel.BlueprintAugmentSlot;
		bool num = InventoryHelper.TryUnequip(base.ViewModel);
		if (num)
		{
			if (flag)
			{
				UISounds.Instance.Sounds.AugmentationsWindow.AugmentOverdriveRemove.Play();
			}
			EventBus.RaiseEvent(delegate(IAugmentUnequipHandler h)
			{
				h.HandleAugmentUnequip();
			});
		}
		return num;
	}

	public void OnInstallButtonClick()
	{
		if (base.ViewModel.ItemSlot.Owner.CanBeControlled())
		{
			m_InstallButtonFade.DisappearAnimation();
			base.ViewModel.ApplyInstallation();
			UISounds.Instance.Sounds.AugmentationsWindow.AugmentInstallButtonClick.Play();
		}
	}

	public OwlcatMultiButton GetInstallButtonEntity()
	{
		return m_InstallButton;
	}

	private void RefreshLockState()
	{
		if (base.ViewModel.BlueprintAugmentSlot != null && base.ViewModel.Unit?.GetOptional<PartUnitBody>()?.Augments.OverdriveSlot == base.ViewModel.BlueprintAugmentSlot)
		{
			m_LockState.SetActiveLayer("Galvanized");
			return;
		}
		bool value = base.ViewModel.IsLocked.Value;
		if (!value)
		{
			switch (base.ViewModel.SlotType)
			{
			case AugmentationsEnum.Forgeworld:
				m_LockState.SetActiveLayer("ForgeForld");
				return;
			case AugmentationsEnum.Mech1:
			case AugmentationsEnum.Mech2:
			case AugmentationsEnum.Mech3:
				m_LockState.SetActiveLayer("AdeptusMechanicus");
				return;
			}
		}
		if (base.ViewModel.HasTrauma.Value && !value)
		{
			m_LockState.SetActiveLayer(0);
		}
		else if (base.ViewModel.Item.Value != null && base.ViewModel.Item.Value.IsNonRemovable)
		{
			m_LockState.SetActiveLayer(0);
		}
		else if (base.ViewModel.IsDefault)
		{
			m_LockState.SetActiveLayer("Default");
		}
		else
		{
			m_LockState.SetActiveLayer(value ? 1 : 2);
		}
	}

	protected string SetupHint(AugmentationsEnum type)
	{
		if (m_ShortDescription == null)
		{
			return "";
		}
		switch (type)
		{
		case AugmentationsEnum.NervousSystem:
			m_ShortDescription.text = UIStrings.Instance.UIAugmentations.SlotNervousSystem.Text;
			return UIStrings.Instance.UIAugmentations.SlotNervousSystem;
		case AugmentationsEnum.PreceptionSystem:
			m_ShortDescription.text = UIStrings.Instance.UIAugmentations.SlotPreceptionSystem.Text;
			return UIStrings.Instance.UIAugmentations.SlotPreceptionSystem;
		case AugmentationsEnum.LeftHand:
			m_ShortDescription.text = UIStrings.Instance.UIAugmentations.SlotLeftHand.Text;
			return UIStrings.Instance.UIAugmentations.SlotLeftHand;
		case AugmentationsEnum.Legs:
			m_ShortDescription.text = UIStrings.Instance.UIAugmentations.SlotLegs.Text;
			return UIStrings.Instance.UIAugmentations.SlotLegs;
		case AugmentationsEnum.InternalSystems:
			m_ShortDescription.text = UIStrings.Instance.UIAugmentations.SlotInternalSystems.Text;
			return UIStrings.Instance.UIAugmentations.SlotInternalSystems;
		case AugmentationsEnum.RightHand:
			m_ShortDescription.text = UIStrings.Instance.UIAugmentations.SlotRightHand.Text;
			return UIStrings.Instance.UIAugmentations.SlotRightHand;
		case AugmentationsEnum.Mech1:
			m_ShortDescription.text = UIStrings.Instance.UIAugmentations.SlotMech1.Text;
			return UIStrings.Instance.UIAugmentations.SlotMech1;
		case AugmentationsEnum.Mech2:
			m_ShortDescription.text = UIStrings.Instance.UIAugmentations.SlotMech2.Text;
			return UIStrings.Instance.UIAugmentations.SlotMech2;
		case AugmentationsEnum.Mech3:
			m_ShortDescription.text = UIStrings.Instance.UIAugmentations.SlotMech3.Text;
			return UIStrings.Instance.UIAugmentations.SlotMech3;
		case AugmentationsEnum.Forgeworld:
			m_ShortDescription.text = UIStrings.Instance.UIAugmentations.SlotForgeWorld.Text;
			return UIStrings.Instance.UIAugmentations.SlotForgeWorld;
		default:
			return string.Empty;
		}
	}
}
