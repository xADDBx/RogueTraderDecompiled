using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.PC;

public class SurfaceActionBarPartAbilitiesPCView : SurfaceActionBarPartAbilitiesBaseView
{
	[SerializeField]
	private OwlcatButton m_CharacterButton;

	[SerializeField]
	private SurfaceActionBarAbilitiesRowView m_SurfaceActionBarAbilitiesViewRowPrefab;

	[SerializeField]
	private Transform m_RowsContainer;

	private List<SurfaceActionBarAbilitiesRowView> m_RowsPool = new List<SurfaceActionBarAbilitiesRowView>();

	[SerializeField]
	private SurfaceActionBarSlotAbilityPCView m_SlotPCView;

	[SerializeField]
	protected WidgetListMVVM m_OverdriveAbilityViewContainer;

	public override void Initialize()
	{
		m_RowsPool = m_RowsContainer.GetComponentsInChildren<SurfaceActionBarAbilitiesRowView>().ToList();
	}

	public override void UpdateGrayscale()
	{
		List<SurfaceActionBarSlotAbilityPCView> list = new List<SurfaceActionBarSlotAbilityPCView>();
		foreach (SurfaceActionBarAbilitiesRowView item3 in m_RowsPool)
		{
			List<IWidgetView> slots = item3.GetSlots();
			if (slots == null)
			{
				continue;
			}
			foreach (IWidgetView item4 in slots)
			{
				if (item4 is SurfaceActionBarSlotAbilityPCView item)
				{
					list.Add(item);
				}
			}
		}
		if (m_OverdriveAbilityViewContainer != null)
		{
			foreach (IWidgetView entry in m_OverdriveAbilityViewContainer.Entries)
			{
				if (entry is SurfaceActionBarSlotAbilityPCView item2)
				{
					list.Add(item2);
				}
			}
		}
		foreach (SurfaceActionBarSlotAbilityPCView item5 in list)
		{
			ActionBarSlotVM actionBarSlotVM = (ActionBarSlotVM)item5.GetViewModel();
			if (actionBarSlotVM.IsPossibleWithoutNetRole.Value && actionBarSlotVM.HasConvert.Value)
			{
				item5.SetGreyscale(actionBarSlotVM.ResourceCount.Value != 0 || actionBarSlotVM.HasAvailableConvert.Value);
			}
			else
			{
				item5.SetGreyscale((actionBarSlotVM.IsPossibleWithoutNetRole.Value && !actionBarSlotVM.IsFake.Value) || actionBarSlotVM.IsInCharScreen);
			}
		}
	}

	protected override void BindViewImplementation()
	{
		if (m_CharacterButton != null)
		{
			UISounds.Instance.SetClickAndHoverSound(m_CharacterButton, UISounds.ButtonSoundsEnum.PlastickSound);
			AddDisposable(m_CharacterButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
				{
					h.HandleOpenCharacterInfoPage(CharInfoPageType.Biography, base.ViewModel.Unit);
				});
			}));
			CheckServiceWindowsBlocked();
		}
		AddDisposable(base.ViewModel.UnitChanged.Subscribe(DrawEntries));
		AddDisposable(base.ViewModel.SlotCountChanged.Subscribe(DrawEntries));
		AddDisposable(base.ViewModel.AugmentationsOverdriveAbilityChanged.Subscribe(DrawEntries));
		AddDisposable(base.ViewModel.CheckServiceWindowsBlocked.Subscribe(CheckServiceWindowsBlocked));
		DrawEntries();
	}

	protected override void DestroyViewImplementation()
	{
		foreach (SurfaceActionBarAbilitiesRowView item in m_RowsPool)
		{
			item.Dispose();
			item.gameObject.SetActive(value: false);
		}
		if (m_OverdriveAbilityViewContainer != null)
		{
			m_OverdriveAbilityViewContainer.Clear();
		}
	}

	private void DrawEntries()
	{
		if (base.ViewModel.Slots.Count != 0)
		{
			MechanicActionBarSlot overdriveAbility;
			List<ActionBarSlotVM> gridSlots = GetGridSlots(out overdriveAbility);
			int num = Mathf.Max(Mathf.CeilToInt((float)gridSlots.Count / (float)base.SlotsInRow), 2);
			List<List<ActionBarSlotVM>> list = new List<List<ActionBarSlotVM>>();
			for (int i = 0; i < num; i++)
			{
				list.Add(new List<ActionBarSlotVM>());
			}
			for (int j = 0; j < gridSlots.Count; j++)
			{
				list[j / base.SlotsInRow].Add(gridSlots[j]);
			}
			if (m_OverdriveAbilityViewContainer != null)
			{
				m_OverdriveAbilityViewContainer.gameObject.SetActive(value: false);
			}
			if (m_OverdriveAbilityViewContainer != null && !(overdriveAbility is MechanicActionBarSlotEmpty) && !RootUIContext.Instance.IsCharacterScreenShown)
			{
				m_OverdriveAbilityViewContainer.Clear();
				m_OverdriveAbilityViewContainer.gameObject.SetActive(value: true);
				List<ActionBarSlotVM> vmCollection = new List<ActionBarSlotVM> { base.ViewModel.OverdriveSlotVM };
				SurfaceActionBarSlotAbilityPCView slotPCView = m_SlotPCView;
				slotPCView.SetClickSound(UISounds.ButtonSoundsEnum.AugmentationsOverdriveClick);
				slotPCView.SetHoverSound(UISounds.ButtonSoundsEnum.AugmentationsOverdriveHover);
				m_OverdriveAbilityViewContainer.DrawEntries(vmCollection, slotPCView);
			}
			for (int k = 0; k < num; k++)
			{
				SurfaceActionBarAbilitiesRowView row = GetRow(k);
				row.gameObject.SetActive(value: true);
				row.Initialize(m_TooltipPlace, new List<Vector2> { Vector2.zero });
				row.DrawEntries(list[k], m_SlotPCView);
			}
			HideRowsFrom(num);
			SetKeyBindings();
		}
	}

	private void SetKeyBindings()
	{
		List<SurfaceActionBarSlotAbilityPCView> list = new List<SurfaceActionBarSlotAbilityPCView>();
		foreach (SurfaceActionBarAbilitiesRowView item2 in m_RowsPool)
		{
			List<IWidgetView> slots = item2.GetSlots();
			if (slots == null)
			{
				continue;
			}
			foreach (IWidgetView item3 in slots)
			{
				if (item3 is SurfaceActionBarSlotAbilityPCView item)
				{
					list.Add(item);
				}
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			list[i].SetKeyBinding(i);
		}
	}

	private SurfaceActionBarAbilitiesRowView GetRow(int i)
	{
		if (i < m_RowsPool.Count)
		{
			return m_RowsPool[i];
		}
		SurfaceActionBarAbilitiesRowView widget = WidgetFactory.GetWidget(m_SurfaceActionBarAbilitiesViewRowPrefab);
		widget.transform.SetParent(m_RowsContainer, worldPositionStays: false);
		m_RowsPool.Add(widget);
		return widget;
	}

	private void HideRowsFrom(int i)
	{
		while (i < m_RowsPool.Count)
		{
			m_RowsPool[i].Dispose();
			m_RowsPool[i].gameObject.SetActive(value: false);
			i++;
		}
	}

	private void CheckServiceWindowsBlocked()
	{
		bool flag = (bool)Game.Instance.Player.ServiceWindowsBlocked || (bool)Game.Instance.Player.CharacterInfoWindowBlocked;
		m_CharacterButton.SetInteractable(!flag);
		AddDisposable(flag ? m_CharacterButton.SetHint(UIStrings.Instance.ExplorationTexts.ExploNotInteractable, "NotInteractable") : m_CharacterButton.SetHint(UIStrings.Instance.MainMenu.CharacterInfo, "OpenCharacterScreen"));
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		DrawEntries();
	}
}
