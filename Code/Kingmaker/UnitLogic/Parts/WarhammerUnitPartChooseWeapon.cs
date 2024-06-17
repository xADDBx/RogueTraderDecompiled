using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class WarhammerUnitPartChooseWeapon : BaseUnitPart, IHashable
{
	[JsonProperty]
	private EntityRef<ItemEntityWeapon> m_WeaponRef;

	public ItemEntityWeapon ChosenWeapon => m_WeaponRef.Entity;

	public void ChooseWeapon(ItemEntityWeapon weapon)
	{
		m_WeaponRef = weapon;
	}

	public void RemoveWeapon()
	{
		m_WeaponRef = null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityRef<ItemEntityWeapon> obj = m_WeaponRef;
		Hash128 val2 = StructHasher<EntityRef<ItemEntityWeapon>>.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}
}
