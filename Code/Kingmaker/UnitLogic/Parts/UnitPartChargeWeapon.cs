using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartChargeWeapon : BaseUnitPart, IHashable
{
	public BlueprintItemWeapon WeaponBlueprint { get; private set; }

	[CanBeNull]
	public WeaponSlot WeaponSlot => base.Owner.Body.FindWeaponSlot((WeaponSlot s) => s.MaybeWeapon?.Blueprint == WeaponBlueprint);

	public UnitPartChargeWeapon Set(BlueprintItemWeapon weapon)
	{
		WeaponBlueprint = weapon;
		return this;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
