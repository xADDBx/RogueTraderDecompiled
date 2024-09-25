using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.PC;

public class SurfaceActionBarWeaponSetPCView : ViewBase<SurfaceActionBarPartWeaponSetVM>
{
	[SerializeField]
	private OwlcatButton m_SwitchWeaponButton;

	[SerializeField]
	private SurfaceActionBarSlotWeaponPCView m_MainHandWeapon;

	[SerializeField]
	private SurfaceActionBarSlotWeaponPCView m_OffHandWeapon;

	[SerializeField]
	private SurfaceActionBarSlotWeaponAbilityPCView m_SlotPCView;

	[SerializeField]
	private GreedyWidgetList m_MainHandWidgetList;

	[SerializeField]
	private GreedyWidgetList m_OffHandWidgetList;

	[SerializeField]
	private GreedyWidgetList m_ComboHandWidgetList;

	[SerializeField]
	private Image m_CurrentSetImage;

	private Action m_SwitchButtonCallback;

	[SerializeField]
	private RectTransform m_LeftSideTooltipPlace;

	private bool m_SetKeyBindings;

	public bool IsCurrent => base.ViewModel?.IsCurrent.Value ?? false;

	private List<Vector2> m_LeftSideTooltipPivots { get; } = new List<Vector2>
	{
		new Vector2(1f, 0f),
		new Vector2(0.9f, 0f),
		new Vector2(0.8f, 0f),
		new Vector2(0.7f, 0f),
		new Vector2(0.6f, 0f)
	};


	public void Initialize(bool setKeyBindings)
	{
		m_SetKeyBindings = setKeyBindings;
	}

	public void SetSwitchButtonCallback(Action callback)
	{
		m_SwitchButtonCallback = callback;
	}

	protected override void BindViewImplementation()
	{
		m_MainHandWeapon.SetTooltipCustomPosition(m_LeftSideTooltipPlace, m_LeftSideTooltipPivots);
		m_OffHandWeapon.SetTooltipCustomPosition(m_LeftSideTooltipPlace, m_LeftSideTooltipPivots);
		AddDisposable(base.ViewModel.IsCurrent.Subscribe(delegate(bool val)
		{
			m_CurrentSetImage.Or(null)?.gameObject.SetActive(val);
		}));
		AddDisposable(base.ViewModel.MainHandWeapon.CombineLatest(base.ViewModel.OffHandWeapon, (ItemSlotVM _, ItemSlotVM _) => true).Subscribe(delegate
		{
			if (base.ViewModel.IsTwoHanded)
			{
				m_MainHandWeapon.Bind(base.ViewModel.MainHandWeapon.Value);
				m_OffHandWeapon.Bind(base.ViewModel.MainHandWeapon.Value);
			}
			else
			{
				m_MainHandWeapon.Bind(base.ViewModel.MainHandWeapon.Value);
				m_OffHandWeapon.Bind(base.ViewModel.OffHandWeapon.Value);
			}
			m_OffHandWeapon.SetFakeMode(base.ViewModel.IsTwoHanded);
		}));
		AddDisposable(UniRxExtensionMethods.Subscribe(base.ViewModel.SlotsUpdated, delegate
		{
			DrawEntries();
		}));
		if (m_SwitchWeaponButton != null)
		{
			AddDisposable(m_SwitchWeaponButton.OnLeftClickAsObservable().Subscribe(OnClick));
		}
		DrawEntries();
	}

	private void OnClick()
	{
		m_SwitchButtonCallback?.Invoke();
	}

	private void DrawEntries()
	{
		m_MainHandWidgetList.Clear();
		m_OffHandWidgetList.Clear();
		m_ComboHandWidgetList.Clear();
		AddDisposable(m_MainHandWidgetList.DrawEntries(base.ViewModel.MainHandSlots, m_SlotPCView));
		AddDisposable(m_OffHandWidgetList.DrawEntries(base.ViewModel.OffHandSlots, m_SlotPCView));
		AddDisposable(m_ComboHandWidgetList.DrawEntries(base.ViewModel.ComboHandsSlots, m_SlotPCView));
		SetActionBarSlotsTooltipCustomPosition(m_MainHandWidgetList);
		SetActionBarSlotsTooltipCustomPosition(m_OffHandWidgetList);
		SetActionBarSlotsTooltipCustomPosition(m_ComboHandWidgetList);
		List<SurfaceActionBarSlotWeaponAbilityPCView> slots = GetSlots();
		for (int i = 0; i < slots.Count; i++)
		{
			slots[i].SetInteractable(m_SetKeyBindings);
			if (m_SetKeyBindings)
			{
				slots[i].SetKeyBinding(i);
			}
		}
	}

	private void SetActionBarSlotsTooltipCustomPosition(GreedyWidgetList widgetList)
	{
		foreach (IWidgetView visibleEntry in widgetList.VisibleEntries)
		{
			if (visibleEntry is SurfaceActionBarSlotWeaponAbilityPCView surfaceActionBarSlotWeaponAbilityPCView)
			{
				surfaceActionBarSlotWeaponAbilityPCView.SetTooltipCustomPosition(m_LeftSideTooltipPlace, m_LeftSideTooltipPivots);
			}
		}
	}

	protected override void DestroyViewImplementation()
	{
		m_MainHandWidgetList.VisibleEntries.ForEach(delegate(IWidgetView s)
		{
			s.BindWidgetVM(null);
		});
		m_MainHandWidgetList.Clear();
		m_OffHandWidgetList.VisibleEntries.ForEach(delegate(IWidgetView s)
		{
			s.BindWidgetVM(null);
		});
		m_OffHandWidgetList.Clear();
		m_ComboHandWidgetList.VisibleEntries.ForEach(delegate(IWidgetView s)
		{
			s.BindWidgetVM(null);
		});
		m_ComboHandWidgetList.Clear();
	}

	public void SwitchWeapon()
	{
		base.ViewModel.SwitchWeapon();
	}

	private List<SurfaceActionBarSlotWeaponAbilityPCView> GetSlots()
	{
		List<SurfaceActionBarSlotWeaponAbilityPCView> list = new List<SurfaceActionBarSlotWeaponAbilityPCView>();
		if ((m_MainHandWidgetList.Or(null)?.Entries?.Any()).GetValueOrDefault())
		{
			list.AddRange(m_MainHandWidgetList.Entries.Select((IWidgetView e) => e as SurfaceActionBarSlotWeaponAbilityPCView));
		}
		if ((m_OffHandWidgetList.Or(null)?.Entries?.Any()).GetValueOrDefault())
		{
			list.AddRange(m_OffHandWidgetList.Entries.Select((IWidgetView e) => e as SurfaceActionBarSlotWeaponAbilityPCView));
		}
		if ((m_ComboHandWidgetList.Or(null)?.Entries?.Any()).GetValueOrDefault())
		{
			list.AddRange(m_ComboHandWidgetList.Entries.Select((IWidgetView e) => e as SurfaceActionBarSlotWeaponAbilityPCView));
		}
		return list;
	}
}
