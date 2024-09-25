using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Controls.Selectable;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.Console;

public class SurfaceActionBarQuickAccessCarouselView : MonoBehaviour, IDisposable
{
	[SerializeField]
	private OwlcatMultiSelectable m_ActiveButton;

	[SerializeField]
	private ActionBarBaseSlotView m_MainSlotView;

	[SerializeField]
	private ActionBarBaseSlotView m_NextSlotView;

	[SerializeField]
	private ActionBarBaseSlotView m_PreviousSlotView;

	[HideInInspector]
	public BoolReactiveProperty IsActive = new BoolReactiveProperty();

	[HideInInspector]
	public BoolReactiveProperty HasSlots = new BoolReactiveProperty();

	private List<ActionBarSlotVM> m_SlotVMs;

	private ReactiveProperty<ActionBarSlotVM> m_MainSlotVM;

	private readonly ReactiveProperty<ActionBarSlotVM> m_NextSlotVM = new ReactiveProperty<ActionBarSlotVM>();

	private readonly ReactiveProperty<ActionBarSlotVM> m_PreviousSlotVM = new ReactiveProperty<ActionBarSlotVM>();

	private CompositeDisposable m_Disposable = new CompositeDisposable();

	private bool m_FindNextFirst = true;

	public IDisposable Initialize(ReactiveProperty<ActionBarSlotVM> mainSlotVM)
	{
		m_MainSlotView.Initialize();
		m_MainSlotView.SetVisible(visible: false);
		m_NextSlotView.Initialize();
		m_PreviousSlotView.Initialize();
		m_MainSlotVM = mainSlotVM;
		m_Disposable.Add(IsActive.Subscribe(OnActive));
		m_Disposable.Add(m_MainSlotVM.Subscribe(OnMainSlotVMChanged));
		m_Disposable.Add(m_NextSlotVM.Subscribe(m_NextSlotView.Bind));
		m_Disposable.Add(m_PreviousSlotVM.Subscribe(m_PreviousSlotView.Bind));
		return this;
	}

	private void OnActive(bool active)
	{
		m_ActiveButton.SetActiveLayer(active ? 1 : 0);
		if (!active)
		{
			m_MainSlotVM.Value = null;
		}
	}

	public void SetSlots(List<ActionBarSlotVM> slotVMs)
	{
		m_SlotVMs = slotVMs.Where((ActionBarSlotVM s) => !s.IsEmpty.Value && s.AbilityData != null).ToList();
		HasSlots.Value = !m_SlotVMs.Empty();
		m_FindNextFirst = true;
		m_MainSlotVM.SetValueAndForceNotify(null);
	}

	private void OnMainSlotVMChanged(ActionBarSlotVM slotVM)
	{
		(m_MainSlotView.GetViewModel() as ActionBarSlotVM)?.OnHoverOff();
		if (slotVM != null)
		{
			List<ActionBarSlotVM> slotVMs = m_SlotVMs;
			if (slotVMs == null || !slotVMs.Contains(slotVM))
			{
				goto IL_0048;
			}
		}
		m_MainSlotView.Bind(slotVM);
		slotVM?.OnHoverOn();
		goto IL_0048;
		IL_0048:
		ActionBarSlotVM actionBarSlotVM;
		ActionBarSlotVM actionBarSlotVM2;
		if (m_FindNextFirst)
		{
			actionBarSlotVM = GetNearSlotVM(next: true);
			if (actionBarSlotVM == m_MainSlotVM.Value)
			{
				actionBarSlotVM = null;
			}
			actionBarSlotVM2 = GetNearSlotVM(next: false);
			if (actionBarSlotVM2 == m_MainSlotVM.Value || actionBarSlotVM2 == actionBarSlotVM)
			{
				actionBarSlotVM2 = null;
			}
		}
		else
		{
			actionBarSlotVM2 = GetNearSlotVM(next: false);
			if (actionBarSlotVM2 == m_MainSlotVM.Value)
			{
				actionBarSlotVM2 = null;
			}
			actionBarSlotVM = GetNearSlotVM(next: true);
			if (actionBarSlotVM == m_MainSlotVM.Value || actionBarSlotVM == actionBarSlotVM2)
			{
				actionBarSlotVM = null;
			}
		}
		m_NextSlotVM.Value = actionBarSlotVM;
		m_PreviousSlotVM.Value = actionBarSlotVM2;
	}

	private ActionBarSlotVM GetNearSlotVM(bool next)
	{
		if (m_SlotVMs.Empty())
		{
			return null;
		}
		int num = m_SlotVMs.IndexOf(m_MainSlotVM.Value);
		if (num == -1 && !next)
		{
			num = 0;
		}
		return m_SlotVMs[GetNearIndex(num, m_SlotVMs.Count, next)];
	}

	private static int GetNearIndex(int current, int count, bool next)
	{
		return (current + count + (next ? 1 : (-1))) % count;
	}

	public void ClickNext()
	{
		if (IsActive.Value)
		{
			m_FindNextFirst = false;
			m_MainSlotVM.Value = GetNearSlotVM(next: true);
			UISounds.Instance.Sounds.Buttons.ButtonHover.Play();
		}
	}

	public void ClickPrevious()
	{
		if (IsActive.Value)
		{
			m_FindNextFirst = true;
			m_MainSlotVM.Value = GetNearSlotVM(next: false);
			UISounds.Instance.Sounds.Buttons.ButtonHover.Play();
		}
	}

	public void Dispose()
	{
		m_Disposable.Clear();
	}

	public void HandleFocusState(bool shouldShowFocus)
	{
		m_ActiveButton.SetActiveLayer((shouldShowFocus && IsActive.Value) ? 1 : 0);
	}
}
