using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Items;

public static class ItemStatHelper
{
	public static int GetSpellLevel(this ItemEntity item)
	{
		if (!(item.Blueprint is BlueprintItemEquipmentUsable))
		{
			return 0;
		}
		return 1;
	}

	public static int GetSpellLevel(this BlueprintItem blueprintItem)
	{
		if (!(blueprintItem is BlueprintItemEquipmentUsable))
		{
			return 0;
		}
		return 1;
	}

	public static int GetCasterLevel(this ItemEntity item)
	{
		if (!(item.Blueprint is BlueprintItemEquipmentUsable))
		{
			return 0;
		}
		return 1;
	}

	public static int GetCasterLevel(this BlueprintItem blueprintItem)
	{
		if (!(blueprintItem is BlueprintItemEquipmentUsable))
		{
			return 0;
		}
		return 1;
	}

	public static int GetCasterLevel(this ItemEntity item, MechanicEntity caster)
	{
		if (item.Blueprint is BlueprintItemEquipmentUsable { Type: var type })
		{
			UnitPartScrollSpecialization optional = caster.GetOptional<UnitPartScrollSpecialization>();
			UnitPartEnhancePotion optional2 = caster.GetOptional<UnitPartEnhancePotion>();
			if (optional == null || type != UsableItemType.Scroll)
			{
				if (optional2 == null || type != UsableItemType.Potion)
				{
					return 1;
				}
				return optional2.classData.Level;
			}
			return optional.SpecializedClassSpellbook.CasterLevel;
		}
		return 0;
	}

	public static float GetProfitFactorCost(this ItemEntity item)
	{
		if (item.Blueprint.ProfitFactorCost > 0f)
		{
			return item.Blueprint.ProfitFactorCost;
		}
		if (item.Blueprint is BlueprintItemEquipmentUsable blueprintItemEquipmentUsable)
		{
			int num = 0;
			switch (blueprintItemEquipmentUsable.Type)
			{
			case UsableItemType.Wand:
				num = 15;
				break;
			case UsableItemType.Scroll:
				num = 25;
				break;
			case UsableItemType.Potion:
				num = 50;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case UsableItemType.Other:
				break;
			}
			return item.GetCasterLevel() * item.GetSpellLevel() * num * blueprintItemEquipmentUsable.Charges;
		}
		return item.Blueprint.ProfitFactorCost;
	}

	public static double GetCostPF(this ItemEntity item)
	{
		return item.Blueprint.ProfitFactorCost;
	}
}
