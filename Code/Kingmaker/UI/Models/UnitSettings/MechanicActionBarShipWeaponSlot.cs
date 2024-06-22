using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.UI.Models.UnitSettings;

public class MechanicActionBarShipWeaponSlot : MechanicActionBarSlotAbility, IHashable
{
	public readonly WeaponSlot WeaponSlot;

	private readonly List<AbilityData> m_AbilityVariants;

	public MechanicActionBarShipWeaponSlot(WeaponSlot weaponSlot, BaseUnitEntity owner)
	{
		WeaponSlot = weaponSlot;
		List<AbilityData> list = weaponSlot.AbilityVariants?.Select((Ability a) => a.Data).ToList();
		if (list != null && list.Count <= 1)
		{
			list = null;
		}
		base.Ability = weaponSlot.ActiveAbility.Data;
		m_AbilityVariants = list;
		base.Unit = owner;
	}

	public override IEnumerable<AbilityData> GetConvertedAbilityData()
	{
		IEnumerable<AbilityData> abilityVariants = m_AbilityVariants;
		return abilityVariants ?? base.GetConvertedAbilityData();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
