using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities.Base;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic;
using Warhammer.SpaceCombat.StarshipLogic.Equipment;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.Items;

public static class ItemsEntityFactory
{
	public static ItemEntity CreateEntity(this BlueprintItem bpItem)
	{
		ItemDlcRestriction component = bpItem.GetComponent<ItemDlcRestriction>();
		if (component != null && (bool)component.ChangeTo && component.IsRestricted)
		{
			bpItem = component.ChangeTo;
		}
		try
		{
			return TryCreateWeapon(bpItem) ?? TryCreateArmor(bpItem) ?? TryCreateShield(bpItem) ?? TryCreateUsable(bpItem) ?? TryCreateMechadendrite(bpItem) ?? TryCreateStarshipWeapon(bpItem) ?? TryCreateStarshipAmmo(bpItem) ?? TryCreateStarshipVoidShieldGenerator(bpItem) ?? TryCreateStarshipPlasmaDrives(bpItem) ?? TryCreateStarshipItemEntity(bpItem) ?? Entity.Initialize(new ItemEntitySimple(bpItem));
		}
		catch (Exception innerException)
		{
			throw new Exception("Failed to create item: " + bpItem.NameSafe(), innerException);
		}
	}

	public static TItemEntity CreateEntity<TItemEntity>(this BlueprintItem bpItem) where TItemEntity : ItemEntity
	{
		return (TItemEntity)bpItem.CreateEntity();
	}

	public static ItemEntity CreateItemCopy(ItemEntity item, int count)
	{
		ItemEntity itemEntity = item.Blueprint.CreateEntity();
		itemEntity.SetCount(count);
		itemEntity.Charges = item.Charges;
		if (item.IsIdentified)
		{
			itemEntity.Identify();
		}
		return itemEntity;
	}

	private static ItemEntity TryCreateWeapon(BlueprintItem bpItem)
	{
		if (!(bpItem is BlueprintItemWeapon bpItem2))
		{
			return null;
		}
		return Entity.Initialize(new ItemEntityWeapon(bpItem2));
	}

	private static ItemEntity TryCreateMechadendrite(BlueprintItem bpItem)
	{
		if (!(bpItem is BlueprintItemMechadendrite bpItem2))
		{
			return null;
		}
		return Entity.Initialize(new ItemEntityMechadendrite(bpItem2));
	}

	private static ItemEntity TryCreateArmor(BlueprintItem bpItem)
	{
		if (!(bpItem is BlueprintItemArmor bpItem2))
		{
			return null;
		}
		return Entity.Initialize(new ItemEntityArmor(bpItem2));
	}

	private static ItemEntity TryCreateShield(BlueprintItem bpItem)
	{
		if (!(bpItem is BlueprintItemShield bpItem2))
		{
			return null;
		}
		return Entity.Initialize(new ItemEntityShield(bpItem2));
	}

	private static ItemEntity TryCreateUsable(BlueprintItem bpItem)
	{
		if (!(bpItem is BlueprintItemEquipmentUsable bpItem2))
		{
			return null;
		}
		return Entity.Initialize(new ItemEntityUsable(bpItem2));
	}

	private static ItemEntity TryCreateStarshipWeapon(BlueprintItem bpItem)
	{
		if (!(bpItem is BlueprintStarshipWeapon bpItem2))
		{
			return null;
		}
		return Entity.Initialize(new ItemEntityStarshipWeapon(bpItem2));
	}

	private static ItemEntity TryCreateStarshipAmmo(BlueprintItem bpItem)
	{
		if (!(bpItem is BlueprintStarshipAmmo bpItem2))
		{
			return null;
		}
		return Entity.Initialize(new ItemEntityStarshipAmmo(bpItem2));
	}

	private static ItemEntity TryCreateStarshipVoidShieldGenerator(BlueprintItem bpItem)
	{
		if (!(bpItem is BlueprintItemVoidShieldGenerator bpItem2))
		{
			return null;
		}
		return Entity.Initialize(new ItemEntityVoidShieldGenerator(bpItem2));
	}

	private static ItemEntity TryCreateStarshipPlasmaDrives(BlueprintItem bpItem)
	{
		if (!(bpItem is BlueprintItemPlasmaDrives bpItem2))
		{
			return null;
		}
		return Entity.Initialize(new ItemEntityPlasmaDrives(bpItem2));
	}

	private static ItemEntity TryCreateStarshipItemEntity(BlueprintItem bpItem)
	{
		if (!(bpItem is BlueprintStarshipItem bpItem2))
		{
			return null;
		}
		return Entity.Initialize(new StarshipItemEntity<BlueprintStarshipItem>(bpItem2));
	}
}
