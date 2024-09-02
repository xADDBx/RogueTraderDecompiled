using System;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Skills;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;

public abstract class InventoryBaseView<TInventoryStash, TInventoryDoll, TInventorySlot> : ViewBase<InventoryVM>, ILevelUpInitiateUIHandler, ISubscriber, IInitializable where TInventoryStash : InventoryStashView where TInventoryDoll : InventoryDollView<TInventorySlot> where TInventorySlot : InventoryEquipSlotView
{
	[Header("Rotation")]
	[SerializeField]
	protected RectTransform m_LeftCanvas;

	[SerializeField]
	protected RectTransform m_RightCanvas;

	[SerializeField]
	private float m_MoveAnimationTime = 0.2f;

	[Header("Character Info")]
	[SerializeField]
	protected CharInfoNameAndPortraitPCView m_NameAndPortraitPCView;

	[SerializeField]
	protected CharInfoLevelClassScoresPCView m_LevelClassScoresView;

	[SerializeField]
	protected CharInfoSkillsAndWeaponsBaseView m_SkillsAndWeaponsView;

	[Header("Other")]
	[SerializeField]
	protected TInventoryDoll m_DollView;

	[SerializeField]
	protected TInventoryStash m_StashView;

	private bool m_IsInit;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_NameAndPortraitPCView.Initialize();
			m_LevelClassScoresView.Initialize();
			m_SkillsAndWeaponsView.Initialize();
			m_DollView.Initialize();
			m_StashView.Initialize();
			m_IsInit = true;
			base.gameObject.SetActive(value: false);
		}
	}

	protected override void BindViewImplementation()
	{
		ShowWindow();
		m_NameAndPortraitPCView.Bind(base.ViewModel.NameAndPortraitVM);
		m_LevelClassScoresView.Bind(base.ViewModel.LevelClassScoresVM);
		m_SkillsAndWeaponsView.Bind(base.ViewModel.CharInfoSkillsAndWeaponsVM);
		StartCoroutine(m_DollView.DelayedBind(base.ViewModel.DollVM, m_MoveAnimationTime));
		m_StashView.Bind(base.ViewModel.StashVM);
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DestroyViewImplementation()
	{
		HideWindow();
		m_DollView.ClearViewIfNeeded();
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
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Inventory);
		});
		EventBus.RaiseEvent(delegate(IFullScreenUIHandlerWorkaround h)
		{
			h.HandleFullScreenUiChangedWorkaround(state: true, FullScreenUIType.Inventory);
		});
		Game.Instance.RequestPauseUi(isPaused: true);
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
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Inventory);
		});
		EventBus.RaiseEvent(delegate(IFullScreenUIHandlerWorkaround h)
		{
			h.HandleFullScreenUiChangedWorkaround(state: false, FullScreenUIType.Inventory);
		});
		Game.Instance.RequestPauseUi(isPaused: false);
	}

	public void HandleLevelUpStart(BaseUnitEntity unit, Action onCommit = null, Action onStop = null, LevelUpState.CharBuildMode mode = LevelUpState.CharBuildMode.LevelUp)
	{
		HideWindow();
	}
}
