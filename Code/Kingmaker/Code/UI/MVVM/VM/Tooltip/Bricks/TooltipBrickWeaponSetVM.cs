using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Weapons;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickWeaponSetVM : TooltipBaseBrickVM
{
	public readonly ItemEntityWeapon Weapon;

	public readonly WeaponSlotVM WeaponSlotVM;

	public readonly AutoDisposingList<CharInfoWeaponSetAbilityVM> Abilities = new AutoDisposingList<CharInfoWeaponSetAbilityVM>();

	public TooltipBrickWeaponSetVM(WeaponSlot weaponSlot)
	{
		Weapon = weaponSlot.Weapon;
		WeaponSlotVM = new WeaponSlotVM(weaponSlot);
		foreach (WeaponAbility weaponAbility in Weapon.Blueprint.WeaponAbilities)
		{
			CharInfoWeaponSetAbilityVM charInfoWeaponSetAbilityVM = new CharInfoWeaponSetAbilityVM(weaponAbility.Ability, Weapon.Blueprint, Weapon.Owner);
			Abilities.Add(charInfoWeaponSetAbilityVM);
			AddDisposable(charInfoWeaponSetAbilityVM);
		}
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		Abilities.Clear();
	}
}
