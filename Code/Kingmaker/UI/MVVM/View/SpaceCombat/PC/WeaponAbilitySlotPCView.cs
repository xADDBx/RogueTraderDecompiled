using Kingmaker.Code.UI.MVVM.View.ActionBar;
using Kingmaker.Code.UI.MVVM.View.ActionBar.PC;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.PC;

public class WeaponAbilitySlotPCView : ActionBarBaseSlotView
{
	[Header("PCSlot")]
	[SerializeField]
	private ActionBarSlotPCView m_SlotPCView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_SlotPCView.Bind(base.ViewModel);
	}
}
