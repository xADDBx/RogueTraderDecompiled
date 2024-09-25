using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.Loot;

public abstract class LootView<TCargoView, TLootCollector, TInteractionSlot, TPlayerStash, TInventoryStash> : ViewBase<LootVM>, IInitializable where TCargoView : InventoryCargoView where TLootCollector : LootCollectorView where TInteractionSlot : InteractionSlotPartView where TPlayerStash : PlayerStashView where TInventoryStash : InventoryStashView
{
	[SerializeField]
	private FadeAnimator m_Animator;

	[Header("Collector")]
	[SerializeField]
	protected TLootCollector m_Collector;

	[Header("InteractionSlot")]
	[SerializeField]
	protected TInteractionSlot m_InteractionSlot;

	[Header("PlayerStash")]
	[SerializeField]
	protected TPlayerStash m_PlayerStash;

	[Header("Stash")]
	[SerializeField]
	protected TInventoryStash m_Inventory;

	[FormerlySerializedAs("m_CargoPCView")]
	[Header("Cargo")]
	[SerializeField]
	protected TCargoView m_Cargo;

	[Header("Leave Zone")]
	[SerializeField]
	private TextMeshProUGUI m_LeaveZoneButtonText;

	[SerializeField]
	private MoveAnimator m_RightWindow;

	[SerializeField]
	private MoveAnimator m_LeftWindow;

	[SerializeField]
	protected FlexibleLensSelectorView m_SelectorView;

	[SerializeField]
	protected FadeAnimator m_BackgroundBlurWithFade;

	[SerializeField]
	private MoveAnimator m_CandlesAnimator;

	[SerializeField]
	private ExitLocationWindowBaseView m_ExitLocationWindow;

	protected readonly ReactiveCommand OnPanelsChanged = new ReactiveCommand();

	private bool m_RightPanelIsShown;

	private bool m_LeftPanelIsShown;

	public void Initialize()
	{
		m_Animator.Initialize();
		m_Collector.Initialize();
		m_InteractionSlot.Initialize();
		m_PlayerStash.Initialize();
		m_Inventory.Initialize();
		m_Cargo.Initialize();
		m_RightWindow.Initialize();
		m_LeftWindow.Initialize();
		m_BackgroundBlurWithFade.Initialize();
		m_CandlesAnimator.Initialize();
		m_ExitLocationWindow.Initialize();
		m_LeaveZoneButtonText.text = UIStrings.Instance.LootWindow.LeaveZone;
	}

	protected override void BindViewImplementation()
	{
		Show();
		m_Collector.Bind(base.ViewModel.LootCollector);
		m_InteractionSlot.Bind(base.ViewModel.InteractionSlot);
		m_PlayerStash.Bind(base.ViewModel.PlayerStash);
		m_Inventory.Bind(base.ViewModel.InventoryStash);
		m_Cargo.Bind(base.ViewModel.CargoInventory);
		AddDisposable(base.ViewModel.ExitLocationWindowVM.Subscribe(m_ExitLocationWindow.Bind));
		if (base.ViewModel.IsOneSlot)
		{
			ShowPanels(showLeft: false, showRight: true, force: true);
		}
		else if (base.ViewModel.IsPlayerStash)
		{
			ShowPanels(showLeft: true, showRight: true, force: true);
		}
		else
		{
			bool value = base.ViewModel.ExtendedView.Value;
			ShowPanels(value, value, force: true);
		}
		AddDisposable(base.ViewModel.ExtendedView.Skip(1).Subscribe(delegate(bool extended)
		{
			ShowPanels(extended, extended);
		}));
		AddDisposable(base.ViewModel.OpenDetailed.Subscribe(delegate
		{
			m_SelectorView.SetNextTab();
		}));
		m_SelectorView.Bind(base.ViewModel.Selector);
		if (m_Cargo.IsBinded)
		{
			AddDisposable(m_Cargo.HasVisibleCargo.Subscribe(m_Cargo.SetList));
		}
		if (base.ViewModel.IsOneSlot)
		{
			m_Inventory.SetFilter(ItemsFilterType.Notable);
		}
		if (base.ViewModel.Mode == LootContextVM.LootWindowMode.ZoneExit)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.LootWindow.CollectAllBeforeLeave.Text, addToLog: false, WarningNotificationFormat.Attention);
			});
		}
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	private void ShowPanels(bool showLeft, bool showRight, bool force = false)
	{
		bool flag = false;
		if (showLeft != m_LeftPanelIsShown || force)
		{
			m_LeftWindow.PlayAnimation(showLeft);
			UISounds.Instance.Sounds.Loot.LootLeftPanelShow.Play();
			m_LeftPanelIsShown = showLeft;
			flag = true;
		}
		if (showRight != m_RightPanelIsShown || force)
		{
			m_RightWindow.PlayAnimation(showRight);
			UISounds.Instance.Sounds.Loot.LootRightPanelHide.Play();
			m_RightPanelIsShown = showRight;
			flag = true;
		}
		HandleBackground(showLeft && showRight);
		if (flag)
		{
			OnPanelsChanged.Execute();
		}
	}

	private void HandleBackground(bool value)
	{
		if (!value)
		{
			m_BackgroundBlurWithFade.DisappearAnimation();
			m_CandlesAnimator.DisappearAnimation();
		}
		else
		{
			m_BackgroundBlurWithFade.AppearAnimation();
			m_CandlesAnimator.AppearAnimation();
		}
	}

	private void Show()
	{
		m_Animator.AppearAnimation();
		UISounds.Instance.Sounds.Loot.LootWindowOpen.Play();
	}

	private void Hide()
	{
		ContextMenuHelper.HideContextMenu();
		m_Animator.DisappearAnimation();
		UISounds.Instance.Sounds.Loot.LootWindowClose.Play();
	}
}
