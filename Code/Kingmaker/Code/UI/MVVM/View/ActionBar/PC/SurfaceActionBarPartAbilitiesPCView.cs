using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
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

	public override void Initialize()
	{
		m_RowsPool = m_RowsContainer.GetComponentsInChildren<SurfaceActionBarAbilitiesRowView>().ToList();
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
	}

	private void DrawEntries()
	{
		int count = base.ViewModel.Slots.Count;
		if (count != 0)
		{
			int num = Mathf.Max(Mathf.CeilToInt((float)count / (float)base.SlotsInRow), 2);
			List<List<ActionBarSlotVM>> list = new List<List<ActionBarSlotVM>>();
			for (int i = 0; i < num; i++)
			{
				list.Add(new List<ActionBarSlotVM>());
			}
			for (int j = 0; j < count; j++)
			{
				list[j / base.SlotsInRow].Add(base.ViewModel.Slots[j]);
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
		List<SurfaceActionBarSlotAbilityPCView> list = m_RowsPool.SelectMany((SurfaceActionBarAbilitiesRowView r) => r.GetSlots()).Cast<SurfaceActionBarSlotAbilityPCView>().ToList();
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
}
