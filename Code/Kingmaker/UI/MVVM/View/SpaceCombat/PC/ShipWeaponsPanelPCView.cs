using Kingmaker.Code.UI.MVVM.VM.SpaceCombat;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints.Slots;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.PC;

public class ShipWeaponsPanelPCView : ViewBase<ShipWeaponsPanelVM>
{
	[Header("Weapon Groups")]
	[SerializeField]
	private WeaponAbilitiesGroupPCView m_PortGroup;

	[SerializeField]
	private WeaponAbilitiesGroupPCView m_ProwGroup;

	[SerializeField]
	private WeaponAbilitiesGroupPCView m_DorsalGroup;

	[SerializeField]
	private WeaponAbilitiesGroupPCView m_StarboardGroup;

	[Header("Ability Groups")]
	[SerializeField]
	private AbilitiesGroupPCView m_AbilitiesGroup;

	[SerializeField]
	private AbilitiesGroupPCView m_SecondAbilitiesGroup;

	protected override void BindViewImplementation()
	{
		m_PortGroup.Bind(base.ViewModel.WeaponAbilitiesGroups[WeaponSlotType.Port]);
		m_ProwGroup.Bind(base.ViewModel.WeaponAbilitiesGroups[WeaponSlotType.Prow]);
		m_DorsalGroup.Bind(base.ViewModel.WeaponAbilitiesGroups[WeaponSlotType.Dorsal]);
		m_StarboardGroup.Bind(base.ViewModel.WeaponAbilitiesGroups[WeaponSlotType.Starboard]);
		m_AbilitiesGroup.Bind(base.ViewModel.AbilitiesGroup);
		m_SecondAbilitiesGroup.Bind(base.ViewModel.AbilitiesGroup);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
