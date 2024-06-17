using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items.Slots;

public class ArmorSlot : ItemSlot, IHashable
{
	public bool HasArmor => HasItem;

	[NotNull]
	public ItemEntityArmor Armor => (ItemEntityArmor)base.Item;

	[CanBeNull]
	public ItemEntityArmor MaybeArmor => (ItemEntityArmor)base.MaybeItem;

	public override bool IsItemSupported(ItemEntity item)
	{
		MechanicEntity owner = base.Owner;
		if (owner != null && owner.IsInCombat)
		{
			return false;
		}
		if (item is ItemEntityArmor armor)
		{
			if (!IsNotRestrictedByRace(armor))
			{
				return item.CanBeEquippedBy(base.Owner);
			}
			return true;
		}
		return false;
	}

	private bool IsNotRestrictedByRace(ItemEntityArmor armor)
	{
		Race? raceRestriction = armor.Blueprint.GetRaceRestriction();
		if (!raceRestriction.HasValue)
		{
			return true;
		}
		return raceRestriction == ((base.Owner as BaseUnitEntity)?.Blueprint.Race?.RaceId).GetValueOrDefault();
	}

	public override bool CanRemoveItem()
	{
		if (base.Owner == null || base.Owner == null || base.Owner.IsInCombat)
		{
			return false;
		}
		return base.CanRemoveItem();
	}

	public ArmorSlot(BaseUnitEntity owner)
		: base(owner)
	{
	}

	public ArmorSlot(JsonConstructorMark _)
		: base(_)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
