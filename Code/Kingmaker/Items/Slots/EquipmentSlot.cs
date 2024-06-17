using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items.Slots;

public class EquipmentSlot<TBlueprintItem> : ItemSlot, IHashable where TBlueprintItem : BlueprintItemEquipment
{
	public EquipmentSlot(BaseUnitEntity owner)
		: base(owner)
	{
	}

	public EquipmentSlot(JsonConstructorMark _)
		: base(_)
	{
	}

	public override bool IsItemSupported(ItemEntity item)
	{
		MechanicEntity owner = base.Owner;
		if (owner != null && owner.IsInCombat)
		{
			return false;
		}
		return item?.Blueprint is TBlueprintItem;
	}

	public override bool CanRemoveItem()
	{
		MechanicEntity owner = base.Owner;
		if (owner != null && owner.IsInCombat)
		{
			return false;
		}
		return base.CanRemoveItem();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
