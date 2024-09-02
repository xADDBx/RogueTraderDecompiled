using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.Surface.InputLayers;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.Console;

public class SurfaceActionBarPartQuickAccessConsoleView : ViewBase<SurfaceActionBarVM>, IUnitDirectHoverUIHandler, ISubscriber, IClickMechanicActionBarSlotHandler, IAbilityTargetSelectionUIHandler, IAbilityOwnerTargetSelectionHandler, ICullFocusHandler
{
	[SerializeField]
	private SurfaceActionBarQuickAccessCarouselView m_HorizontalCarousel;

	[SerializeField]
	private SurfaceActionBarQuickAccessCarouselView m_VerticalCarousel;

	[SerializeField]
	private MoveAnimator m_MoveAnimator;

	[SerializeField]
	private RectTransform m_TooltipPlace;

	[SerializeField]
	private SurfaceActionBarPartWeaponsConsoleView m_WeaponsConsoleView;

	[SerializeField]
	private SurfaceActionBarPartAbilitiesConsoleView m_AbilitiesConsoleView;

	[SerializeField]
	private RectTransform m_EmptyMoveSlot;

	[Header("Hints")]
	[SerializeField]
	private ConsoleHint m_DPadSurfaceHint;

	[SerializeField]
	private ConsoleHint m_DPadCombatHint;

	[SerializeField]
	private ConsoleHint m_DPadInternalHint;

	[SerializeField]
	private ConsoleHint m_ConfirmHint;

	[SerializeField]
	private ConsoleHint m_InfoHint;

	[SerializeField]
	private ConsoleHint m_AbilitiesHint;

	private IDisposable m_Disposable;

	private InputLayer m_InputLayer;

	private readonly BoolReactiveProperty m_IsActive = new BoolReactiveProperty();

	private bool m_ShowTooltip;

	private readonly BoolReactiveProperty m_HasSlot = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CanActivate = new BoolReactiveProperty();

	private IDisposable m_CanActivateSubscription;

	private SurfaceActionBarQuickAccessCarouselView m_ActiveCarouselView;

	public void Initialize()
	{
		m_MoveAnimator.Initialize();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(m_HorizontalCarousel.Initialize(base.ViewModel.QuickAccessSlot));
		AddDisposable(m_VerticalCarousel.Initialize(base.ViewModel.QuickAccessSlot));
		m_WeaponsConsoleView.Bind(base.ViewModel.Weapons);
		AddDisposable(base.ViewModel.Weapons.CurrentSet.Subscribe(delegate(SurfaceActionBarPartWeaponSetVM set)
		{
			if (set != null)
			{
				m_HorizontalCarousel.SetSlots(set.AllSlots);
				m_Disposable?.Dispose();
				m_Disposable = ObservableExtensions.Subscribe(set.SlotsUpdated, delegate
				{
					m_HorizontalCarousel.SetSlots(set.AllSlots);
				});
			}
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.Consumables.UnitChanged, delegate
		{
			OnDeclineClicked(default(InputActionEventData));
			m_VerticalCarousel.SetSlots(base.ViewModel.Consumables.Slots);
		}));
		AddDisposable(base.ViewModel.QuickAccessSlot.Subscribe(OnQuickAccessSlotChanged));
		CreateInput();
		AddDisposable(m_IsActive.Subscribe(Activate));
		AddDisposable(m_IsActive.CombineLatest(m_HasSlot, (bool isActive, bool hasSlot) => new { isActive, hasSlot }).Subscribe(value =>
		{
			m_EmptyMoveSlot.Or(null)?.gameObject.SetActive(!value.isActive && !value.hasSlot);
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DestroyViewImplementation()
	{
		m_InputLayer = null;
		m_CanActivateSubscription?.Dispose();
		m_Disposable?.Dispose();
	}

	private void CreateInput()
	{
		m_InputLayer = new InputLayer
		{
			ContextName = "SurfaceActionBarPartQuickAccessConsoleView"
		};
		AddDisposable(m_DPadInternalHint.BindCustomAction(new List<int> { 6, 7, 4, 5 }, m_InputLayer));
		AddDisposable(m_InputLayer.AddButton(OnUpClicked, 6, m_VerticalCarousel.HasSlots));
		AddDisposable(m_InputLayer.AddButton(OnDownClicked, 7, m_VerticalCarousel.HasSlots));
		AddDisposable(m_InputLayer.AddButton(OnLeftClicked, 4, m_HorizontalCarousel.HasSlots));
		AddDisposable(m_InputLayer.AddButton(OnRightClicked, 5, m_HorizontalCarousel.HasSlots));
		AddDisposable(m_InputLayer.AddAxis2D(OnLeftStickMoved, 0, 1, repeat: false));
		AddDisposable(m_InputLayer.AddAxis2D(OnRightStickMoved, 2, 3, repeat: false));
		AddDisposable(m_InfoHint.Bind(m_InputLayer.AddButton(ToggleTooltip, 19, m_HasSlot, InputActionEventType.ButtonJustReleased)));
		AddDisposable(m_InputLayer.AddButton(OnDeclineClicked, 9));
		AddDisposable(m_ConfirmHint.Bind(m_InputLayer.AddButton(OnConfirmClicked, 8, m_CanActivate)));
		AddDisposable(m_InputLayer.AddButton(CheckPingCoop, 8, m_CanActivate.Not().ToReactiveProperty()));
		AddDisposable(m_AbilitiesHint.Bind(m_InputLayer.AddButton(OnAbilitiesActivate, 11)));
		m_WeaponsConsoleView.AddInput(m_InputLayer);
	}

	private void Activate(bool active)
	{
		if (active)
		{
			m_MoveAnimator.AppearAnimation();
			UISounds.Instance.Sounds.ActionBar.DPadShow.Play();
			Game.Instance.ClickEventsController.ClearPointerMode();
			GamePad.Instance.PushLayer(m_InputLayer);
			Game.Instance.CursorController.SetActive(active: true);
		}
		else
		{
			m_MoveAnimator.DisappearAnimation();
			UISounds.Instance.Sounds.ActionBar.DPadHide.Play();
			GamePad.Instance.PopLayer(m_InputLayer);
		}
	}

	private void OnQuickAccessSlotChanged(ActionBarSlotVM slotVM)
	{
		m_HasSlot.Value = slotVM != null;
		m_CanActivateSubscription?.Dispose();
		if (m_HasSlot.Value)
		{
			m_CanActivateSubscription = slotVM?.IsPossibleActive.Subscribe(delegate(bool a)
			{
				m_CanActivate.Value = a;
			});
		}
		else
		{
			m_CanActivate.Value = false;
		}
		if (m_ShowTooltip)
		{
			((MonoBehaviour)null).ShowTooltip(slotVM?.Tooltip?.Value, new TooltipConfig
			{
				TooltipPlace = m_TooltipPlace,
				PriorityPivots = new List<Vector2> { Vector2.zero }
			});
		}
		else
		{
			TooltipHelper.HideTooltip();
		}
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip = !RootUIContext.Instance.TooltipIsShown;
		OnQuickAccessSlotChanged(base.ViewModel.QuickAccessSlot.Value);
	}

	public void AddInput(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> enable, bool inCombat)
	{
		AddDisposable((inCombat ? m_DPadCombatHint : m_DPadSurfaceHint).BindCustomAction(new List<int> { 6, 7, 4, 5 }, inputLayer));
		AddDisposable(inputLayer.AddButton(OnUpClicked, 6, enable));
		AddDisposable(inputLayer.AddButton(OnDownClicked, 7, enable, InputActionEventType.ButtonJustReleased));
		AddDisposable(inputLayer.AddButton(OnLeftClicked, 4, enable));
		AddDisposable(inputLayer.AddButton(OnRightClicked, 5, enable));
		m_WeaponsConsoleView.AddInput(inputLayer);
	}

	private void OnUpClicked(InputActionEventData data)
	{
		if (!m_IsActive.Value)
		{
			m_IsActive.Value = true;
		}
		m_HorizontalCarousel.IsActive.Value = false;
		m_VerticalCarousel.IsActive.Value = true;
		m_VerticalCarousel.ClickNext();
	}

	private void OnDownClicked(InputActionEventData data)
	{
		if (!m_IsActive.Value)
		{
			m_IsActive.Value = true;
		}
		m_HorizontalCarousel.IsActive.Value = false;
		m_VerticalCarousel.IsActive.Value = true;
		m_VerticalCarousel.ClickPrevious();
	}

	private void OnLeftClicked(InputActionEventData data)
	{
		if (!m_IsActive.Value)
		{
			m_IsActive.Value = true;
		}
		m_VerticalCarousel.IsActive.Value = false;
		m_HorizontalCarousel.IsActive.Value = true;
		m_HorizontalCarousel.ClickPrevious();
	}

	private void OnRightClicked(InputActionEventData data)
	{
		if (!m_IsActive.Value)
		{
			m_IsActive.Value = true;
		}
		m_VerticalCarousel.IsActive.Value = false;
		m_HorizontalCarousel.IsActive.Value = true;
		m_HorizontalCarousel.ClickNext();
	}

	private void OnLeftStickMoved(InputActionEventData data, Vector2 vector)
	{
		SurfaceMainInputLayer.MoveCursor(vector);
	}

	private void OnRightStickMoved(InputActionEventData data, Vector2 vector)
	{
		SurfaceMainInputLayer.MoveRotateCamera(vector);
	}

	private void OnConfirmClicked(InputActionEventData data)
	{
		UISounds.Instance.Sounds.Buttons.ButtonClick.Play();
		base.ViewModel.QuickAccessSlot.Value.OnMainClick();
	}

	private void CheckPingCoop(InputActionEventData data)
	{
		if (m_CanActivate.Value)
		{
			return;
		}
		ActionBarSlotVM slot = base.ViewModel.QuickAccessSlot.Value;
		if (PhotonManager.NetGame.CurrentState != NetGame.State.Playing || slot.MechanicActionBarSlot.Unit.IsMyNetRole())
		{
			return;
		}
		PhotonManager.Ping.PressPing(delegate
		{
			if (!string.IsNullOrWhiteSpace(slot.MechanicActionBarSlot.KeyName))
			{
				PhotonManager.Ping.PingActionBarAbility(slot.MechanicActionBarSlot.KeyName, slot.MechanicActionBarSlot.Unit, slot.Index, slot.WeaponSlotType);
			}
		});
	}

	private void OnDeclineClicked(InputActionEventData data)
	{
		m_HorizontalCarousel.IsActive.Value = false;
		m_VerticalCarousel.IsActive.Value = false;
		m_IsActive.Value = false;
	}

	private void OnAbilitiesActivate(InputActionEventData data)
	{
		OnDeclineClicked(data);
		m_AbilitiesConsoleView.Activate();
	}

	public void HandleHoverChange(AbstractUnitEntityView unitEntityView, bool isHover)
	{
		if ((bool)SettingsRoot.Game.TurnBased.AutoSelectWeaponAbility && (Game.Instance.Player.IsInCombat || GamePad.Instance.CursorEnabled) && !m_HorizontalCarousel.IsActive.Value && !m_VerticalCarousel.IsActive.Value && !RootUIContext.Instance.IsInitiativeTrackerActive)
		{
			if (!isHover)
			{
				base.ViewModel.QuickAccessSlot.Value = null;
			}
			else if ((!Game.Instance.Player.IsInCombat || Game.Instance.TurnController.IsPlayerTurn) && !(Game.Instance.SelectedAbilityHandler?.Ability != null))
			{
				base.ViewModel.QuickAccessSlot.Value = base.ViewModel.GetSuitableSlot(unitEntityView);
			}
		}
	}

	public void HandleClickMechanicActionBarSlot(MechanicActionBarSlot ability)
	{
		m_IsActive.Value = false;
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		m_HorizontalCarousel.IsActive.Value = false;
		m_VerticalCarousel.IsActive.Value = false;
	}

	public void HandleOwnerAbilitySelected(AbilityData ability)
	{
		m_HorizontalCarousel.IsActive.Value = false;
		m_VerticalCarousel.IsActive.Value = false;
	}

	public void HandleRemoveFocus()
	{
		if (m_HorizontalCarousel.IsActive.Value)
		{
			m_ActiveCarouselView = m_HorizontalCarousel;
		}
		else if (m_VerticalCarousel.IsActive.Value)
		{
			m_ActiveCarouselView = m_VerticalCarousel;
		}
		if (m_ActiveCarouselView != null)
		{
			m_ActiveCarouselView.HandleFocusState(shouldShowFocus: false);
		}
	}

	public void HandleRestoreFocus()
	{
		if (m_ActiveCarouselView != null)
		{
			m_ActiveCarouselView.HandleFocusState(shouldShowFocus: true);
		}
		m_ActiveCarouselView = null;
	}
}
