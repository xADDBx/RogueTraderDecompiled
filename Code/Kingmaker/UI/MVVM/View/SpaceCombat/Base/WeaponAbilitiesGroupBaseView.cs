using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat.Components;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.Base;

[RequireComponent(typeof(CanvasGroup))]
public class WeaponAbilitiesGroupBaseView : ViewBase<AbilitiesGroupVM>
{
	[SerializeField]
	private TextMeshProUGUI m_GroupLabel;

	[SerializeField]
	private bool m_HasMultipleSlotsGroups;

	[ConditionalShow("m_HasMultipleSlotsGroups")]
	[SerializeField]
	private CanvasGroup m_SingleSlotGroup;

	[ConditionalShow("m_HasMultipleSlotsGroups")]
	[SerializeField]
	private CanvasGroup m_DoubleSlotGroup;

	[SerializeField]
	private RectTransform m_SlotsContainer;

	[SerializeField]
	private ActionBarBaseSlotView m_SlotView;

	[SerializeField]
	private float m_AnimationDuration = 0.5f;

	protected readonly List<ActionBarBaseSlotView> SlotsList = new List<ActionBarBaseSlotView>();

	private bool m_IsActive = true;

	private Sequence m_Animation;

	private CanvasGroup m_CanvasGroup;

	private CanvasGroup CanvasGroup => m_CanvasGroup = (m_CanvasGroup ? m_CanvasGroup : GetComponent<CanvasGroup>());

	protected override void BindViewImplementation()
	{
		if ((bool)m_GroupLabel)
		{
			m_GroupLabel.text = base.ViewModel.GroupLabel;
		}
		AddDisposable(base.ViewModel.Slots.ObserveAdd().Subscribe(delegate
		{
			DrawSlots();
		}));
		AddDisposable(base.ViewModel.Slots.ObserveRemove().Subscribe(delegate
		{
			DrawSlots();
		}));
		AddDisposable(base.ViewModel.Slots.ObserveReset().Subscribe(delegate
		{
			Clear();
		}));
		DrawSlots();
	}

	protected override void DestroyViewImplementation()
	{
		Clear();
	}

	private void DrawSlots()
	{
		Clear();
		foreach (ActionBarSlotVM slot in base.ViewModel.Slots)
		{
			ActionBarBaseSlotView widget = WidgetFactory.GetWidget(m_SlotView);
			widget.Bind(slot);
			widget.transform.SetParent(m_SlotsContainer, worldPositionStays: false);
			SlotsList.Add(widget);
		}
		if (m_HasMultipleSlotsGroups)
		{
			bool flag = base.ViewModel.Slots.Count > 1;
			m_SingleSlotGroup.alpha = (flag ? 0f : 1f);
			m_DoubleSlotGroup.alpha = (flag ? 1f : 0f);
		}
	}

	private void Clear()
	{
		SlotsList.ForEach(WidgetFactory.DisposeWidget);
		SlotsList.Clear();
	}

	private void Show()
	{
		if (!m_IsActive)
		{
			m_Animation?.Complete();
			m_Animation = DOTween.Sequence();
			m_Animation.Append(CanvasGroup.DOFade(1f, m_AnimationDuration));
			m_Animation.Join(m_GroupLabel.DOFade(1f, m_AnimationDuration));
			m_Animation.AppendCallback(delegate
			{
				m_IsActive = true;
			});
			m_Animation.SetUpdate(isIndependentUpdate: true).Play();
		}
	}

	private void Hide()
	{
		if (m_IsActive)
		{
			m_Animation?.Complete();
			m_Animation = DOTween.Sequence();
			m_Animation.Append(CanvasGroup.DOFade(0f, m_AnimationDuration));
			m_Animation.Join(m_GroupLabel.DOFade(0f, m_AnimationDuration));
			m_Animation.AppendCallback(delegate
			{
				m_IsActive = false;
			});
			m_Animation.SetUpdate(isIndependentUpdate: true).Play();
		}
	}
}
