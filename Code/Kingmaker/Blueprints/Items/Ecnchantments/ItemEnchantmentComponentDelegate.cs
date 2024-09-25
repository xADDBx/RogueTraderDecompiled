using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Ecnchantments;

[TypeId("6f965cfa9b11477408bbb6268a8f9d85")]
public abstract class ItemEnchantmentComponentDelegate<TItemEntity> : EntityFactComponentDelegate<TItemEntity>, IHashable where TItemEntity : ItemEntity
{
	protected ItemEnchantment Enchantment => (ItemEnchantment)base.Fact;

	protected MechanicEntity Wielder => Owner.Wielder;

	protected new TItemEntity Owner
	{
		get
		{
			TItemEntity owner = base.Owner;
			if (owner is ItemEntityShield itemEntityShield)
			{
				if (typeof(TItemEntity) == typeof(ItemEntityArmor))
				{
					return itemEntityShield.ArmorComponent as TItemEntity;
				}
				if (typeof(TItemEntity) == typeof(ItemEntityWeapon))
				{
					return itemEntityShield.WeaponComponent as TItemEntity;
				}
			}
			return owner;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
[TypeId("6f965cfa9b11477408bbb6268a8f9d85")]
public abstract class ItemEnchantmentComponentDelegate : ItemEnchantmentComponentDelegate<ItemEntity>, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
