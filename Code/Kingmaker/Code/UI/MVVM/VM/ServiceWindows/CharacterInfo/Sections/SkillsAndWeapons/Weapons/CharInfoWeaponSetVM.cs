using System;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Items;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Weapons;

public class CharInfoWeaponSetVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<EquipSlotVM> SelectedHand = new ReactiveProperty<EquipSlotVM>();

	public readonly ReactiveCollection<CharInfoWeaponSetAbilityVM> Abilities = new ReactiveCollection<CharInfoWeaponSetAbilityVM>();

	public EquipSlotVM Primary { get; set; }

	public EquipSlotVM Secondary { get; set; }

	public CharInfoWeaponSetVM(HandsEquipmentSet equipmentSet)
	{
		AddDisposable(Primary = new EquipSlotVM(EquipSlotType.PrimaryHand, equipmentSet.PrimaryHand));
		AddDisposable(Secondary = new EquipSlotVM(EquipSlotType.SecondaryHand, equipmentSet.SecondaryHand, -1, Primary));
		AddDisposable(SelectedHand.Subscribe(delegate
		{
			UpdateAbilities();
		}));
		UpdateSelectedHand();
	}

	protected override void DisposeImplementation()
	{
	}

	public void SelectHand(EquipSlotVM slotVm)
	{
		if (slotVm != null)
		{
			SelectedHand.Value = slotVm;
		}
	}

	private void UpdateSelectedHand()
	{
		if (Primary.ItemSlot.HasItem)
		{
			SelectedHand.Value = Primary;
		}
		else
		{
			SelectedHand.Value = (Secondary.ItemSlot.HasItem ? Secondary : null);
		}
	}

	private void UpdateAbilities()
	{
		Abilities.Clear();
		if (!(SelectedHand.Value?.Item.Value is ItemEntityWeapon itemEntityWeapon))
		{
			return;
		}
		UIConfig uIConfig = BlueprintRoot.Instance.UIConfig;
		foreach (WeaponAbility weaponAbility in itemEntityWeapon.Blueprint.WeaponAbilities)
		{
			if (weaponAbility.Ability != uIConfig.ReloadAbility)
			{
				CharInfoWeaponSetAbilityVM charInfoWeaponSetAbilityVM = new CharInfoWeaponSetAbilityVM(weaponAbility.Ability, itemEntityWeapon.Blueprint);
				Abilities.Add(charInfoWeaponSetAbilityVM);
				AddDisposable(charInfoWeaponSetAbilityVM);
			}
		}
	}
}
