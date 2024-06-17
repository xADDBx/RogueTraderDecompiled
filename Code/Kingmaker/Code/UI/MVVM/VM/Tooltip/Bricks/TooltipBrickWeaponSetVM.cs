using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Weapons;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickWeaponSetVM : TooltipBaseBrickVM
{
	public HandSlot HandSlot;

	public bool IsPrimary;

	public ItemEntityWeapon Weapon;

	public EquipSlotVM EquipSlot;

	public readonly AutoDisposingList<CharInfoWeaponSetAbilityVM> Abilities = new AutoDisposingList<CharInfoWeaponSetAbilityVM>();

	public TooltipBrickWeaponSetVM(HandSlot handSlot, bool isPrimary)
	{
		HandSlot = handSlot;
		IsPrimary = isPrimary;
		AddDisposable(EquipSlot = new EquipSlotVM(EquipSlotType.PrimaryHand, handSlot));
		Weapon = (isPrimary ? HandSlot.HandsEquipmentSet.PrimaryHand.Weapon : HandSlot.HandsEquipmentSet.SecondaryHand.Weapon);
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
