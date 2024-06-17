using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Weapon;

public class AmmoSlot : ItemSlot, IHashable
{
	[JsonProperty]
	public readonly WeaponSlot WeaponSlot;

	public bool HasAmmo => Ammo != null;

	public ItemEntityStarshipAmmo Ammo => base.MaybeItem as ItemEntityStarshipAmmo;

	public override bool IsItemSupported(ItemEntity item)
	{
		if (item is ItemEntityStarshipAmmo itemEntityStarshipAmmo && WeaponSlot.Weapon != null)
		{
			return itemEntityStarshipAmmo.Blueprint.WeaponType == WeaponSlot.Weapon.Blueprint.WeaponType;
		}
		return false;
	}

	public AmmoSlot(BaseUnitEntity owner, WeaponSlot weaponSlot)
		: base(owner)
	{
		WeaponSlot = weaponSlot;
	}

	public AmmoSlot(JsonConstructorMark _)
		: base(_)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<WeaponSlot>.GetHash128(WeaponSlot);
		result.Append(ref val2);
		return result;
	}
}
