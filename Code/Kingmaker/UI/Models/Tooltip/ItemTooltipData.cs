using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UI.Models.Tooltip;

public class ItemTooltipData
{
	public ItemEntity Item;

	public ItemEntityUsable Usable;

	public BlueprintItem BlueprintItem;

	public BlueprintItemEquipmentUsable BlueprintUsable;

	public readonly Dictionary<DamageType, string> OtherDamage = new Dictionary<DamageType, string>();

	public readonly Dictionary<StatType, int> StatBonus = new Dictionary<StatType, int>();

	public readonly List<UIUtilityItem.RestrictionData> Restrictions = new List<UIUtilityItem.RestrictionData>();

	public BlueprintAbility BlueprintAbility;

	public Dictionary<TooltipElement, string> Texts { get; } = new Dictionary<TooltipElement, string>();


	public Dictionary<TooltipElement, string> AddTexts { get; } = new Dictionary<TooltipElement, string>();


	public Dictionary<TooltipElement, CompareData> CompareData { get; } = new Dictionary<TooltipElement, CompareData>();


	public Dictionary<StatType, string> BonusDamageFromStat { get; } = new Dictionary<StatType, string>();


	public List<UIUtilityItem.UIAbilityData> Abilities { get; } = new List<UIUtilityItem.UIAbilityData>();


	public ItemTooltipData(ItemEntity item)
	{
		Item = item;
		Usable = item as ItemEntityUsable;
	}

	public ItemTooltipData(BlueprintItem blueprintItem)
	{
		BlueprintItem = blueprintItem;
		BlueprintUsable = blueprintItem as BlueprintItemEquipmentUsable;
	}

	public string GetText(TooltipElement type)
	{
		return Texts.Get(type);
	}

	public string GetAddText(TooltipElement type)
	{
		return AddTexts.Get(type);
	}

	public string GetLabel(TooltipElement type)
	{
		return UIStrings.Instance.TooltipsElementLabels.GetLabel(type);
	}

	public CompareData GetCompareData(TooltipElement type)
	{
		return CompareData.Get(type);
	}

	public string GetOtherDamage(DamageType type)
	{
		return OtherDamage.Get(type);
	}
}
