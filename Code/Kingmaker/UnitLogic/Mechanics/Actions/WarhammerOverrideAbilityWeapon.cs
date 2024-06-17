using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("c12d0b47a00427c47be7af83bb98bf3c")]
public class WarhammerOverrideAbilityWeapon : MechanicEntityFactComponentDelegate, IHashable
{
	[SerializeField]
	private BlueprintItemWeaponReference m_Weapon;

	public BlueprintItemWeapon Weapon => m_Weapon?.Get();

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
