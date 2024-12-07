using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("7746b2e63dc54f7b83694cd007f55eef")]
public class WarhammerContextActionReloadWeapon : ContextAction
{
	public bool PartialReload;

	[ShowIf("PartialReload")]
	public int ReloadAmount;

	public bool ReloadBothWeapons;

	[HideIf("ReloadBothWeapons")]
	public bool ReloadWithWeaponSlot;

	[HideIf("ReloadBothWeapons")]
	public ContextPropertyName WeaponSlotFromContextProperty;

	public int OverloadAmmoAmount;

	protected override void RunAction()
	{
		PartUnitBody partUnitBody = base.Target.Entity?.GetBodyOptional();
		if (ReloadBothWeapons)
		{
			if (partUnitBody != null)
			{
				ReloadHandEquipmentSet(partUnitBody, 0);
				ReloadHandEquipmentSet(partUnitBody, 1);
			}
		}
		else if (ReloadWithWeaponSlot)
		{
			int num = base.Context[WeaponSlotFromContextProperty];
			if (num >= 0)
			{
				ReloadEquipmentSlot(partUnitBody, num);
			}
		}
		else
		{
			ItemEntityWeapon weapon = base.Context.SourceAbilityContext?.Ability.Weapon ?? base.Target.Entity?.GetFirstWeapon();
			ReloadWeapon(weapon);
		}
	}

	private void ReloadHandEquipmentSet(PartUnitBody body, int index)
	{
		ReloadWeapon(body.HandsEquipmentSets[index].PrimaryHand.MaybeItem as ItemEntityWeapon);
		ReloadWeapon(body.HandsEquipmentSets[index].SecondaryHand.MaybeItem as ItemEntityWeapon);
	}

	private void ReloadEquipmentSlot(PartUnitBody body, int index)
	{
		ReloadWeapon(body.EquipmentSlots[index].MaybeItem as ItemEntityWeapon);
	}

	private void ReloadWeapon([CanBeNull] ItemEntityWeapon weapon)
	{
		if (weapon != null && weapon.Blueprint.WarhammerMaxAmmo != -1)
		{
			weapon.CurrentAmmo = OverloadAmmoAmount + (PartialReload ? Math.Max(weapon.CurrentAmmo + ReloadAmount, weapon.Blueprint.WarhammerMaxAmmo) : weapon.Blueprint.WarhammerMaxAmmo);
			weapon.CurrentUsedBarrel = 0;
		}
	}

	public override string GetCaption()
	{
		return (PartialReload ? $"Reload {ReloadAmount} ammo to" : "Reload all ammo of") + " " + (ReloadBothWeapons ? "both weapons" : "current weapon");
	}
}
