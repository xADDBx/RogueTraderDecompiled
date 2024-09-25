using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar.PC;

public class SurfaceActionBarSlotWeaponAbilityPCView : SurfaceActionBarSlotWeaponAbilityView
{
	[Header("PCSlot")]
	[SerializeField]
	private ActionBarSlotPCView m_SlotPCView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_SlotPCView.Bind(base.ViewModel);
		AddDisposable(m_SlotPCView.OnPointerEnterAsObservable.Subscribe(delegate
		{
			OnPointerEnter();
		}));
		AddDisposable(m_SlotPCView.OnPointerExitAsObservable.Subscribe(delegate
		{
			OnPointerExit();
		}));
	}

	private void OnPointerEnter()
	{
		if (base.ViewModel.IsReload.Value && (bool)m_ReloadAmmoContainer)
		{
			m_ReloadAmmoContainerVisibility.SetVisible(visible: false);
		}
	}

	private void OnPointerExit()
	{
		if (base.ViewModel.IsReload.Value && (bool)m_ReloadAmmoContainer)
		{
			m_ShowAmmoContainer.SetValueAndForceNotify(m_ShowAmmoContainer.Value);
		}
	}

	public void SetKeyBinding(int index)
	{
		m_SlotPCView.SetKeyBinding(index);
	}

	public override void SetTooltipCustomPosition(RectTransform rectTransform, List<Vector2> pivots = null)
	{
		m_SlotPCView.SetTooltipCustomPosition(rectTransform, pivots);
	}
}
