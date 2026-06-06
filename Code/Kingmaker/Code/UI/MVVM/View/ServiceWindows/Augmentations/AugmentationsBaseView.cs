using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Augmentations;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.DollRoom;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.View.ShipCustomization;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Augmentations;

public abstract class AugmentationsBaseView<TInventoryStash, TInventorySlot> : ViewBase<AugmentationsVM>, ILevelUpInitiateUIHandler, ISubscriber, IInitializable
{
	[SerializeField]
	private float m_MoveAnimationTime = 0.2f;

	[Header("Other")]
	[SerializeField]
	protected TInventoryStash m_StashView;

	[Space]
	[Header("Slots")]
	[SerializeField]
	protected TInventorySlot m_ForgeWorld;

	[FormerlySerializedAs("m_MechSlot1")]
	[SerializeField]
	protected TInventorySlot m_MechSlot1Pascal;

	[FormerlySerializedAs("m_MechSlot2")]
	[SerializeField]
	protected TInventorySlot m_MechSlot2Pascal;

	[FormerlySerializedAs("m_MechSlot3")]
	[SerializeField]
	protected TInventorySlot m_MechSlot3Pascal;

	[SerializeField]
	protected TInventorySlot m_MechSlot1Manipulus;

	[SerializeField]
	protected TInventorySlot m_MechSlot2Manipulus;

	[SerializeField]
	protected TInventorySlot m_MechSlot3Manipulus;

	[SerializeField]
	protected TInventorySlot m_OverchargeSlot;

	[SerializeField]
	protected TInventorySlot m_InternalSystems;

	[SerializeField]
	protected TInventorySlot m_RightHand;

	[SerializeField]
	protected TInventorySlot m_NervousSystem;

	[SerializeField]
	protected TInventorySlot m_PreceptionSystem;

	[SerializeField]
	protected TInventorySlot m_LeftHand;

	[SerializeField]
	protected TInventorySlot m_Legs;

	[SerializeField]
	protected GameObject m_ForgeWorldSlot;

	[FormerlySerializedAs("m_MechSlots")]
	[SerializeField]
	protected GameObject m_MechSlotsPascal;

	[SerializeField]
	protected GameObject m_MechSlotsManipulus;

	[SerializeField]
	protected TextMeshProUGUI m_MetallizationLabel;

	[SerializeField]
	protected TextMeshProUGUI m_MetallizationValue;

	[SerializeField]
	protected TextMeshProUGUI m_ViewOnlyLabel;

	[SerializeField]
	protected GameObject m_ViewOnlyGameobject;

	[SerializeField]
	protected OwlcatMultiButton m_OverdriveLabelGameObject;

	[SerializeField]
	protected TextMeshProUGUI m_OverdriveLabel;

	[SerializeField]
	protected TextMeshProUGUI m_ScreenNameHeaderLabel;

	[SerializeField]
	protected DollRoomTargetController m_CharacterController;

	private bool m_IsInit;

	public AugmentationsDollRoom AugmentationsDollRoom => UIDollRooms.Instance?.AugmentationsDollRoom;

	public virtual void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
			AugmentationsDollRoom.Initialize(m_CharacterController);
			base.gameObject.SetActive(value: false);
		}
	}

	protected override void BindViewImplementation()
	{
		ShowWindow();
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(base.ViewModel.Metallization.Subscribe(delegate(string value)
		{
			m_MetallizationValue.text = value;
		}));
		AddDisposable(base.ViewModel.IsForgeWorldUnit.Subscribe(delegate(bool value)
		{
			m_ForgeWorldSlot.SetActive(value);
		}));
		AddDisposable(base.ViewModel.IsPasqal.Subscribe(delegate(bool value)
		{
			m_MechSlotsPascal.SetActive(value);
		}));
		AddDisposable(base.ViewModel.IsManipulus.Subscribe(delegate(bool value)
		{
			m_MechSlotsManipulus.SetActive(value);
		}));
		AddDisposable(base.ViewModel.HandleOverdriveLabelsUpdate.Subscribe(OnOverdriveLabelsUpdate));
		OnOverdriveLabelsUpdate();
		m_MetallizationLabel.text = UIStrings.Instance.UIAugmentations.MetallizationLabel;
		m_ViewOnlyGameobject.SetActive(AugmentationsVM.AugmentsViewOnly);
		m_ViewOnlyLabel.text = UIStrings.Instance.UIAugmentations.ViewOnlyLabel;
		m_ScreenNameHeaderLabel.text = UIStrings.Instance.UIAugmentations.AugmentationsScreenNameHeaderLabel;
	}

	protected override void DestroyViewImplementation()
	{
		HideWindow();
	}

	public void HandleLevelUpStart(BaseUnitEntity unit, Action onCommit = null, Action onStop = null, LevelUpState.CharBuildMode mode = LevelUpState.CharBuildMode.LevelUp)
	{
		HideWindow();
	}

	private void ShowWindow()
	{
		base.gameObject.SetActive(value: true);
		UISounds.Instance.Sounds.AugmentationsWindow.OpenWindow.Play();
		UISounds.Instance.Play(UISounds.Instance.Sounds.AugmentationsWindow.AugmentAmbientLoopStart, isButton: false, playAnyway: true);
		OnShow();
	}

	protected void HideWindow()
	{
		OnHide();
		ContextMenuHelper.HideContextMenu();
		base.gameObject.SetActive(value: false);
		UISounds.Instance.Sounds.AugmentationsWindow.CloseWindow.Play();
		UISounds.Instance.Play(UISounds.Instance.Sounds.AugmentationsWindow.AugmentAmbientLoopStop, isButton: false, playAnyway: true);
	}

	private void OnShow()
	{
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Augmentations);
		});
		EventBus.RaiseEvent(delegate(IFullScreenUIHandlerWorkaround h)
		{
			h.HandleFullScreenUiChangedWorkaround(state: true, FullScreenUIType.Augmentations);
		});
		try
		{
			if (!AugmentationsDollRoom)
			{
				return;
			}
			AugmentationsDollRoom.Show();
		}
		catch (Exception ex)
		{
			PFLog.UI.Exception(ex);
		}
		Game.Instance.RequestPauseUi(isPaused: true);
	}

	private void OnHide()
	{
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Augmentations);
		});
		EventBus.RaiseEvent(delegate(IFullScreenUIHandlerWorkaround h)
		{
			h.HandleFullScreenUiChangedWorkaround(state: false, FullScreenUIType.Augmentations);
		});
		SwitchOffDoll();
		Game.Instance.RequestPauseUi(isPaused: false);
	}

	private void SwitchOffDoll()
	{
		try
		{
			if ((bool)AugmentationsDollRoom)
			{
				AugmentationsDollRoom.Hide();
			}
		}
		catch (Exception ex)
		{
			PFLog.UI.Exception(ex);
		}
	}

	public void OnOverdriveLabelsUpdate()
	{
		if (base.ViewModel.Unit.Value == null)
		{
			return;
		}
		if (base.ViewModel.Unit.Value.Body.Augments.OverdriveAbility == null)
		{
			m_OverdriveLabel.text = UIStrings.Instance.UIAugmentations.NoOverchargedLabel;
			AddDisposable(m_OverdriveLabelGameObject.SetHint(UIStrings.Instance.UIAugmentations.AbilityGranted, null, default(Color), shouldShow: false));
			if (m_OverchargeSlot is AugmentationsEquipSlotPCView component)
			{
				AddDisposable(component.SetHint(UIStrings.Instance.UIAugmentations.SelectAugment));
			}
		}
		else
		{
			m_OverdriveLabel.text = UIStrings.Instance.UIAugmentations.OverchargedLabel;
			AddDisposable(m_OverdriveLabelGameObject.SetHint(UIStrings.Instance.UIAugmentations.AbilityGranted));
			if (m_OverchargeSlot is AugmentationsEquipSlotPCView component2)
			{
				AddDisposable(component2.SetHint(UIStrings.Instance.UIAugmentations.SelectAugment, null, default(Color), shouldShow: false));
			}
		}
	}
}
