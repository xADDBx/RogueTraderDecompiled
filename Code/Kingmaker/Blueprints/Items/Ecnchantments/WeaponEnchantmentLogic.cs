using Kingmaker.Blueprints.Attributes;
using Kingmaker.Items;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Ecnchantments;

[AllowedOn(typeof(BlueprintWeaponEnchantment))]
public abstract class WeaponEnchantmentLogic : ItemEnchantmentComponentDelegate<ItemEntityWeapon>, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
