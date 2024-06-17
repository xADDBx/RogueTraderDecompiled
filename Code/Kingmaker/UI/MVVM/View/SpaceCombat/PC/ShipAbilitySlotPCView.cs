using Kingmaker.Code.UI.MVVM.View.ActionBar.PC;
using Kingmaker.UI.MVVM.View.SpaceCombat.Base;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.PC;

public class ShipAbilitySlotPCView : ShipAbilitySlotBaseView
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
