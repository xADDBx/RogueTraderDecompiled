using Kingmaker.Code.UI.MVVM.View.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.SpaceCombat.Components;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.Base;

public class AbilitiesGroupBaseView : ViewBase<AbilitiesGroupVM>
{
	[SerializeField]
	protected ActionBarBaseSlotView[] m_AbilitySlots;

	protected override void BindViewImplementation()
	{
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
	}

	private void DrawSlots()
	{
		int count = m_AbilitySlots.Length - base.ViewModel.Slots.Count;
		base.ViewModel.UpdateEmptySlots(count);
		for (int i = 0; i < m_AbilitySlots.Length; i++)
		{
			if (i < base.ViewModel.Slots.Count)
			{
				m_AbilitySlots[i].gameObject.SetActive(value: true);
				m_AbilitySlots[i].Bind(base.ViewModel.Slots[i]);
			}
			else
			{
				m_AbilitySlots[i].gameObject.SetActive(value: false);
			}
		}
	}

	private void Clear()
	{
		m_AbilitySlots.ForEach(delegate(ActionBarBaseSlotView slot)
		{
			slot.Unbind();
		});
	}
}
