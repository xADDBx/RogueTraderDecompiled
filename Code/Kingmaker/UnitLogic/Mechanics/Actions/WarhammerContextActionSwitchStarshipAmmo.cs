using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("18f04b6b29193e84faec69aae8e87c65")]
public class WarhammerContextActionSwitchStarshipAmmo : ContextAction
{
	[SerializeField]
	private BlueprintStarshipWeaponReference m_Weapon;

	[SerializeField]
	private BlueprintStarshipAmmoReference m_Ammo;

	[SerializeField]
	private bool m_ReloadInstantly;

	public override string GetCaption()
	{
		return "Switch " + m_Weapon?.Get()?.name + " ammo to " + m_Ammo?.Get()?.name;
	}

	protected override void RunAction()
	{
		if (!(base.Target.Entity is StarshipEntity starshipEntity))
		{
			Element.LogError(this, "Target starship is missing");
			return;
		}
		WeaponSlot weaponSlot = starshipEntity.Hull.WeaponSlots.Find((WeaponSlot x) => x.Weapon.Blueprint == m_Weapon?.Get());
		if (weaponSlot == null)
		{
			Element.LogError(this, "Target hasn't required weapon");
		}
		else
		{
			weaponSlot.EquipAmmo(Entity.Initialize(new ItemEntityStarshipAmmo(m_Ammo)), m_ReloadInstantly);
		}
	}
}
