using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Inspect;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models.SettingsUI;
using Kingmaker.UI.MVVM.View.Bark.PC;
using Kingmaker.UI.MVVM.View.ShipCustomization;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Augmentations.PC;

public class AugmentationsPCView : AugmentationsBaseView<AugmentationsInventoryStashView, AugmentationsEquipSlotPCView>, IForceRebindOverdriveBlock, ISubscriber, IDollCharacterDragUIHandler, IAgumentationsDollRotationHandler, IUnitOverdriveAugmentHandler
{
	[SerializeField]
	private OwlcatMultiButton m_ResetDollButton;

	[SerializeField]
	private TextMeshProUGUI m_ResetDollButtonLabel;

	[SerializeField]
	private OwlcatMultiButton m_InfoButton;

	[SerializeField]
	private InspectPCView m_InspectPCView;

	[SerializeField]
	private MoveAnimator m_InspectPCViewAnimation;

	[SerializeField]
	private float m_AppearStartPosition;

	[SerializeField]
	private StarSystemSpaceBarksHolderPCView m_StarSystemSpaceBarksHolderPCView;

	public override void Initialize()
	{
		base.Initialize();
		m_StashView.Initialize();
		m_InspectPCViewAnimation.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.PrevCharacter.name, base.ViewModel.SelectPrevCharacter));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.NextCharacter.name, base.ViewModel.SelectNextCharacter));
		m_InspectPCView.Bind(base.ViewModel.InspectVM);
		AddDisposable(base.ViewModel.InspectVM.Tooltip.Subscribe(delegate(TooltipBaseTemplate tooltip)
		{
			if (tooltip != null)
			{
				Vector3 position = m_InspectPCViewAnimation.gameObject.transform.position;
				position.x = m_AppearStartPosition;
				m_InspectPCViewAnimation.gameObject.transform.position = position;
				DelayedInvoker.InvokeInFrames(delegate
				{
					m_InspectPCViewAnimation.AppearAnimation();
				}, 1);
			}
		}));
		m_StashView.Bind(base.ViewModel.StashVM);
		AddDisposable(base.ViewModel.Unit?.Subscribe(delegate
		{
			RefreshView();
		}));
		AddDisposable(m_ResetDollButton.OnPointerClickAsObservable().Subscribe(delegate
		{
			OnResetDollButtonClickedHandler();
		}));
		AddDisposable(m_InfoButton.OnPointerClickAsObservable().Subscribe(delegate
		{
			AugmentsInfoShowHandler();
		}));
		m_ResetDollButtonLabel.text = UIStrings.Instance.ShipCustomization.ToDefaultPosition;
		if (base.ViewModel.StarSystemSpaceBarksHolderVM != null)
		{
			m_StarSystemSpaceBarksHolderPCView.Bind(base.ViewModel.StarSystemSpaceBarksHolderVM);
		}
		m_ResetDollButton.gameObject.SetActive(value: false);
		UpdateSlots();
	}

	private void RefreshView()
	{
		if (!(base.AugmentationsDollRoom == null) && base.ViewModel.Unit.Value != null)
		{
			try
			{
				base.AugmentationsDollRoom.SetupUnit(base.ViewModel.Unit.Value);
			}
			catch (Exception ex)
			{
				PFLog.UI.Exception(ex);
			}
			UpdateSlots();
		}
	}

	private void UpdateSlots()
	{
		BindSlot(m_NervousSystem);
		BindSlot(m_ForgeWorld);
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
		BindOverrideSlot(m_OverchargeSlot);
	}

	private void BindSlot(AugmentationsEquipSlotPCView slot)
	{
		AugmentationsSlotVM augmentationsSlotVM = base.ViewModel.AllAugmentSlots.FirstOrDefault((AugmentationsSlotVM b) => b.BlueprintAugmentSlot == slot.SlotTypeReference.Get() && !b.IsOverchargeSlot);
		slot.transform.parent.gameObject.SetActive(augmentationsSlotVM != null);
		if (augmentationsSlotVM != null && !slot.IsBinded)
		{
			slot.Bind(augmentationsSlotVM);
		}
	}

	private void BindOverrideSlot(AugmentationsEquipSlotPCView slot)
	{
		UnitAugments augments = base.ViewModel.UnitAugments;
		slot.MarkAsOverchargeSlotView();
		AugmentationsSlotVM augmentationsSlotVM = base.ViewModel.AllAugmentSlots.FirstOrDefault((AugmentationsSlotVM b) => b.BlueprintAugmentSlot == augments.OverdriveSlot);
		if (augmentationsSlotVM != null && augmentationsSlotVM.BlueprintAugmentSlot == null)
		{
			AugmentationsSlotVM augmentationsSlotVM2 = base.ViewModel.AllAugmentSlots.FirstOrDefault((AugmentationsSlotVM b) => b.IsOverchargeSlot);
			slot.Bind(augmentationsSlotVM2);
			augmentationsSlotVM2?.RefreshOverchargeAbility.Execute();
		}
		else
		{
			slot.Bind(augmentationsSlotVM);
			augmentationsSlotVM?.RefreshOverchargeAbility.Execute();
		}
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

	public void HandleForceRebindOverdriveBlock()
	{
		BindOverrideSlot(m_OverchargeSlot);
	}

	public void OnResetDollButtonClickedHandler()
	{
		UIDollRooms.Instance.AugmentationsDollRoom.RotateToDefaultPosition();
	}

	public void AugmentsInfoShowHandler()
	{
		EventBus.RaiseEvent(delegate(IUnitClickUIHandler h)
		{
			h.HandleUnitConsoleInvoke(base.ViewModel.Unit.Value);
		});
	}

	public void StartDrag()
	{
		m_ResetDollButton.gameObject.SetActive(value: true);
	}

	public void EndDrag()
	{
	}

	public void HandleOnRotationStop()
	{
		m_ResetDollButton.gameObject.SetActive(value: false);
	}
}
