using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement;

public class CargoManagementBaseView<TInventoryStash, TInventoryCargo> : ViewBase<CargoManagementVM>, IInitializable where TInventoryStash : InventoryStashView where TInventoryCargo : InventoryCargoView
{
	[Header("Character Info")]
	[SerializeField]
	public ShipNameAndPortraitPCView ShipNameAndPortraitPCView;

	[Header("Other")]
	[SerializeField]
	protected TInventoryStash m_StashView;

	[SerializeField]
	protected TInventoryCargo m_CargoView;

	private bool m_IsInit;

	public virtual void Initialize()
	{
		if (!m_IsInit)
		{
			ShipNameAndPortraitPCView.Initialize();
			m_CargoView.Initialize();
			m_StashView.Initialize();
			m_IsInit = true;
			base.gameObject.SetActive(value: false);
		}
	}

	protected override void BindViewImplementation()
	{
		ShowWindow();
		ShipNameAndPortraitPCView.Bind(base.ViewModel.ShipNameAndPortraitVM);
		m_StashView.Bind(base.ViewModel.StashVM);
		m_CargoView.Bind(base.ViewModel.InventoryCargoVM);
		m_CargoView.m_ListContentFadeAnimator.AppearAnimation();
	}

	protected override void DestroyViewImplementation()
	{
		HideWindow();
	}

	private void ShowWindow()
	{
		base.gameObject.SetActive(value: true);
		UISounds.Instance.Sounds.Inventory.InventoryOpen.Play();
		OnShow();
	}

	private void OnShow()
	{
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.CargoManagement);
		});
		EventBus.RaiseEvent(delegate(IFullScreenUIHandlerWorkaround h)
		{
			h.HandleFullScreenUiChangedWorkaround(state: true, FullScreenUIType.CargoManagement);
		});
	}

	private void HideWindow()
	{
		OnHide();
		ContextMenuHelper.HideContextMenu();
		base.gameObject.SetActive(value: false);
		UISounds.Instance.Sounds.Inventory.InventoryClose.Play();
	}

	private void OnHide()
	{
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.CargoManagement);
		});
		EventBus.RaiseEvent(delegate(IFullScreenUIHandlerWorkaround h)
		{
			h.HandleFullScreenUiChangedWorkaround(state: false, FullScreenUIType.CargoManagement);
		});
		Game.Instance.RequestPauseUi(isPaused: false);
	}
}
