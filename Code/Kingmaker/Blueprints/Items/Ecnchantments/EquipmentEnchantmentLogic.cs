using Kingmaker.Blueprints.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Ecnchantments;

[AllowedOn(typeof(BlueprintEquipmentEnchantment))]
public abstract class EquipmentEnchantmentLogic : ItemEnchantmentComponentDelegate, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
