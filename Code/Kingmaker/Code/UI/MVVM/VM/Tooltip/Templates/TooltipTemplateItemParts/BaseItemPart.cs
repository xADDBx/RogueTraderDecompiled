using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks.Utils;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.UI;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.Visual.Critters;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates.TooltipTemplateItemParts;

public class BaseItemPart : TooltipBaseTemplate
{
	protected readonly ItemEntity Item;

	protected readonly BlueprintItem BlueprintItem;

	protected readonly ItemTooltipData ItemTooltipData;

	protected readonly ItemTooltipData CompareItemTooltipData;

	protected bool HasCompareItem => CompareItemTooltipData != null;

	public BaseItemPart(ItemEntity item, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null)
	{
		Item = item;
		BlueprintItem = item.Blueprint;
		ItemTooltipData = itemTooltipData;
		CompareItemTooltipData = compareItemTooltipData;
	}

	public BaseItemPart(BlueprintItem blueprintItem, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null)
	{
		BlueprintItem = blueprintItem;
		ItemTooltipData = itemTooltipData;
		CompareItemTooltipData = compareItemTooltipData;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		string itemName = GetItemName();
		string text = ItemTooltipData.GetText(TooltipElement.Subname);
		ItemEntity item = Item;
		Sprite image = ((item != null) ? ObjectExtensions.Or(item.Icon, null) : null) ?? SimpleBlueprintExtendAsObject.Or(BlueprintItem, null)?.Icon;
		if (type == TooltipTemplateType.Tooltip)
		{
			AddRestrictions(list, type);
		}
		list.Add(new TooltipBrickEntityHeader(itemName, image, hasUpgrade: false, text));
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		ItemsItemOrigin origin = BlueprintItem?.Origin ?? ItemsItemOrigin.None;
		Sprite iconByOrigin = UIConfig.Instance.UIIcons.CargoTooltipIcons.GetIconByOrigin(origin);
		string labelByOrigin = UIStrings.Instance.CargoTexts.GetLabelByOrigin(origin);
		string rightLine = string.Empty;
		if (!string.IsNullOrEmpty(labelByOrigin))
		{
			string tooltipItemFooterFormat = UIConfig.Instance.TooltipItemFooterFormat;
			string text = ItemTooltipData.GetText(TooltipElement.CargoVolume);
			string arg = string.Format(UIStrings.Instance.Tooltips.ItemFooterLabel, labelByOrigin);
			rightLine = string.Format(tooltipItemFooterFormat, text, arg);
		}
		List<ITooltipBrick> list2 = new List<ITooltipBrick>();
		string endTurn = GetEndTurn(BlueprintItem);
		Sprite moveEndPoints = UIConfig.Instance.UIIcons.TooltipIcons.MoveEndPoints;
		string attackAbilityGroupCooldown = GetAttackAbilityGroupCooldown(BlueprintItem);
		Sprite actionEndPoints = UIConfig.Instance.UIIcons.TooltipIcons.ActionEndPoints;
		if (!string.IsNullOrWhiteSpace(endTurn) || !string.IsNullOrWhiteSpace(attackAbilityGroupCooldown))
		{
			TextFieldParams textFieldParams = new TextFieldParams
			{
				FontColor = UIConfig.Instance.TooltipColors.Default,
				FontStyles = FontStyles.Strikethrough
			};
			list2.Add(new TooltipBrickTripleText(endTurn, attackAbilityGroupCooldown, string.Empty, moveEndPoints, actionEndPoints, null, textFieldParams, textFieldParams, textFieldParams));
		}
		if (list2.Count > 0)
		{
			list.Add(new TooltipBrickSeparator(TooltipBrickElementType.Medium));
			list.AddRange(list2);
		}
		list.Add(new TooltipBrickItemFooter(string.Empty, rightLine, iconByOrigin));
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddNotRemovable(list, type);
		AddDescription(list, type);
		if (type == TooltipTemplateType.Info)
		{
			AddRestrictions(list, type);
		}
		return list;
	}

	protected string GetItemName()
	{
		StringBuilder stringBuilder = new StringBuilder(ItemTooltipData.GetText(TooltipElement.Name));
		ItemEntity item = Item;
		if (item != null && item.Count > 1)
		{
			stringBuilder.Append($" (x{Item?.Count})");
		}
		return stringBuilder.ToString();
	}

	protected bool TryAddIconStatValue(List<ITooltipBrick> bricks, TooltipElement element, Sprite icon = null, TooltipBrickIconStatValueType type = TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType bgrType = TooltipBrickIconStatValueType.Normal)
	{
		string text = ItemTooltipData.GetText(element);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		string addText = ItemTooltipData.GetAddText(element);
		string label = ItemTooltipData.GetLabel(element);
		return TryAddIconStatValue(bricks, label, text, addText, icon, type, bgrType);
	}

	protected bool TryAddIconAndName(List<ITooltipBrick> bricks, TooltipElement element, Sprite icon)
	{
		string text = ItemTooltipData.GetText(element);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		bricks.Add(new TooltipBrickPFIconAndName(icon, text));
		return true;
	}

	protected bool TryAddIconStatValue(List<ITooltipBrick> bricks, string label, string value, string addValue, Sprite icon, TooltipBrickIconStatValueType type = TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType bgrType = TooltipBrickIconStatValueType.Normal)
	{
		if (string.IsNullOrEmpty(value))
		{
			return false;
		}
		bricks.Add(new TooltipBrickIconStatValue(label, value, addValue, icon, type, bgrType));
		return true;
	}

	protected ComparisonResult CompareValues(int baseValue, int otherValue)
	{
		if (baseValue > otherValue)
		{
			return ComparisonResult.Greater;
		}
		if (baseValue < otherValue)
		{
			return ComparisonResult.Less;
		}
		return ComparisonResult.Equal;
	}

	protected void AddDamage(List<ITooltipBrick> bricks)
	{
		TooltipElement type = (string.IsNullOrEmpty(ItemTooltipData.GetText(TooltipElement.EquipDamage)) ? TooltipElement.Damage : TooltipElement.EquipDamage);
		string text = ItemTooltipData.GetText(TooltipElement.BaseDamage);
		string text2 = ItemTooltipData.GetText(type);
		if (text.IsNullOrEmpty())
		{
			text = text2;
		}
		string text3 = ItemTooltipData.GetText(TooltipElement.Penetration) + "%";
		if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text3))
		{
			ComparisonResult comparisonLeft = ComparisonResult.Equal;
			ComparisonResult comparisonRight = ComparisonResult.Equal;
			if (HasCompareItem)
			{
				CompareData compareData = ItemTooltipData.GetCompareData(type);
				CompareData compareData2 = CompareItemTooltipData.GetCompareData(type);
				comparisonLeft = CompareValues(compareData.Value, compareData2.Value);
				CompareData compareData3 = ItemTooltipData.GetCompareData(TooltipElement.Penetration);
				CompareData compareData4 = CompareItemTooltipData.GetCompareData(TooltipElement.Penetration);
				comparisonRight = CompareValues(compareData3.Value, compareData4.Value);
			}
			bool highlightLeft = false;
			bool highlightRight = false;
			if (BlueprintItem.PrototypeLink is BlueprintItemWeapon { CanBeUsedInGame: not false } blueprintItemWeapon && BlueprintItem is BlueprintItemWeapon blueprintItemWeapon2)
			{
				highlightLeft = blueprintItemWeapon2.WarhammerDamage > blueprintItemWeapon.WarhammerDamage;
				highlightRight = blueprintItemWeapon2.WarhammerPenetration > blueprintItemWeapon.WarhammerPenetration;
			}
			Sprite damage = UIConfig.Instance.UIIcons.TooltipIcons.Damage;
			Sprite penetration = UIConfig.Instance.UIIcons.TooltipIcons.Penetration;
			bricks.Add(new TooltipBrickTwoColumnsStat(ItemTooltipData.GetLabel(TooltipElement.Damage), ItemTooltipData.GetLabel(TooltipElement.Penetration), text, text3, damage, penetration, comparisonLeft, comparisonRight, highlightLeft, highlightRight));
		}
	}

	protected void AddRestrictions(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		if (type == TooltipTemplateType.Tooltip && ItemTooltipData.Restrictions.All((UIUtilityItem.RestrictionData r) => r.CanEquip))
		{
			return;
		}
		if (ItemTooltipData.Restrictions.Any() && type == TooltipTemplateType.Info)
		{
			bricks.Add(new TooltipBrickSpace());
			bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.Prerequisites, TooltipTitleType.H2));
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = BlueprintItem.IsFamiliarItem();
		for (int i = 0; i < ItemTooltipData.Restrictions.Count; i++)
		{
			UIUtilityItem.RestrictionData restrictionData = ItemTooltipData.Restrictions[i];
			if (flag)
			{
				bricks.Add((type == TooltipTemplateType.Info) ? new TooltipBrickSeparator() : new TooltipBrickSeparator(TooltipBrickElementType.Small));
				flag = false;
			}
			if (restrictionData.Inverted)
			{
				if ((type == TooltipTemplateType.Info || restrictionData.RestrictionItems.Any((UIUtilityItem.RestrictionItem item) => !item.MeetPrerequisite)) && !flag2 && !flag4)
				{
					bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.NoFeature, TooltipTitleType.H3));
					flag2 = true;
					flag3 = false;
				}
			}
			else if (type == TooltipTemplateType.Info && !flag3 && !flag4)
			{
				bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.PrerequisiteFeatures, TooltipTitleType.H3));
				flag2 = false;
				flag3 = true;
			}
			bool flag5 = false;
			for (int j = 0; j < restrictionData.RestrictionItems.Count; j++)
			{
				UIUtilityItem.RestrictionItem restrictionItem = restrictionData.RestrictionItems[j];
				if (restrictionItem.MeetPrerequisite && type != TooltipTemplateType.Info)
				{
					continue;
				}
				if (flag5)
				{
					if (restrictionData.Inverted)
					{
						bricks.Add((type == TooltipTemplateType.Info) ? new TooltipBrickSeparator() : new TooltipBrickSeparator(TooltipBrickElementType.Small));
						flag5 = false;
					}
					else
					{
						int num = ((type == TooltipTemplateType.Info) ? 100 : 70);
						bricks.Add(new TooltipBrickText($"<size={num}%>{UIStrings.Instance.Tooltips.or.Text}</size>"));
						flag5 = false;
					}
				}
				if (restrictionItem.UnitFact != null)
				{
					if (restrictionItem.UnitFact is BlueprintFeature feature)
					{
						bricks.Add(new TooltipBrickFeature(feature, isHeader: false, restrictionItem.MeetPrerequisite, type == TooltipTemplateType.Info));
					}
					else
					{
						bricks.Add(new TooltipBrickFeature(restrictionItem.UnitFact.Name, null, isHeader: false, restrictionItem.MeetPrerequisite, type == TooltipTemplateType.Info));
					}
				}
				else
				{
					TooltipBrickIconStatValueType tooltipBrickIconStatValueType = ((!restrictionItem.MeetPrerequisite) ? TooltipBrickIconStatValueType.Negative : TooltipBrickIconStatValueType.Normal);
					bricks.Add(new TooltipBrickIconStatValue(restrictionItem.Key, restrictionItem.Value, null, null, tooltipBrickIconStatValueType, tooltipBrickIconStatValueType));
				}
				flag5 = true;
				flag = true;
			}
		}
	}

	protected void AddAbilities(List<ITooltipBrick> bricks)
	{
		if (!ItemTooltipData.Abilities.Any())
		{
			return;
		}
		bricks.Add(new TooltipBrickSpace());
		foreach (UIUtilityItem.UIAbilityData ability in ItemTooltipData.Abilities)
		{
			TooltipTemplateAbility tooltipTemplateAbility = new TooltipTemplateAbility(ability.BlueprintAbility, BlueprintItem);
			Sprite icon = ability.Icon;
			UIUtilityItem.UIPatternData patternData = ability.PatternData;
			string name = ability.Name;
			string costAP = ability.CostAP;
			TooltipBaseTemplate tooltip = tooltipTemplateAbility;
			bricks.Add(new TooltipBrickIconPattern(icon, patternData, name, costAP, null, null, tooltip));
			if (!ability.UIProperties.Any())
			{
				continue;
			}
			bricks.Add(new TooltipBricksGroupStart());
			foreach (UIProperty uIProperty in ability.UIProperties)
			{
				bricks.Add(new TooltipBrickIconStatValue(uIProperty.Name, uIProperty.PropertyValue?.ToString() ?? string.Empty, uIProperty.Description));
			}
			bricks.Add(new TooltipBricksGroupEnd());
		}
	}

	protected void AddItemStatBonuses(List<ITooltipBrick> bricks)
	{
		if (ItemTooltipData.StatBonus.Empty())
		{
			return;
		}
		foreach (KeyValuePair<StatType, int> bonus in ItemTooltipData.StatBonus)
		{
			LocalizedString localizedString = BlueprintRoot.Instance.LocalizedTexts.Stats.Entries.FirstOrDefault((Entry e) => e.Stat == bonus.Key)?.Text;
			string name = ((localizedString != null) ? ((string)localizedString) : "");
			TooltipBrickIconStatValueType type = ((bonus.Value > 0) ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
			bricks.Add(new TooltipBrickIconStatValue(name, UIUtility.AddSign(bonus.Value), null, null, type));
		}
	}

	private void AddNotRemovable(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		if (Item.IsNonRemovable)
		{
			bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.IsNotRemovable, TooltipTitleType.H5));
		}
	}

	protected void AddReplenishing(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		string text = ItemTooltipData.GetText(TooltipElement.Replenishing);
		if (!string.IsNullOrEmpty(text))
		{
			bricks.Add(new TooltipBrickSpace());
			bricks.Add(new TooltipBrickText(text, TooltipTextType.Italic));
		}
	}

	protected void AddDescription(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		string text = ItemTooltipData.GetText(TooltipElement.ShortDescription);
		string text2 = ItemTooltipData.GetText(TooltipElement.ArtisticDescription);
		string text3 = ItemTooltipData.GetText(TooltipElement.Description) + ItemTooltipData.GetText(TooltipElement.LongDescription);
		string text4 = ItemTooltipData.GetText(TooltipElement.EnchantmentsDescription);
		List<string> additionalDescription = TooltipTemplateUtils.GetAdditionalDescription(BlueprintItem);
		switch (type)
		{
		case TooltipTemplateType.Tooltip:
		{
			string text6 = text;
			if (string.IsNullOrEmpty(text6))
			{
				text6 = text3;
				if (string.IsNullOrEmpty(text6))
				{
					text6 = text2;
					if (string.IsNullOrEmpty(text6) && additionalDescription.Count == 0)
					{
						return;
					}
				}
			}
			text6 = TooltipTemplateUtils.AggregateDescription(text6, additionalDescription);
			bricks.Add(new TooltipBrickSpace());
			bricks.Add(new TooltipBrickText(text6, TooltipTextType.Paragraph));
			break;
		}
		case TooltipTemplateType.Info:
			if (!string.IsNullOrEmpty(text2) && !text3.Equals(text2) && !text.Equals(text2))
			{
				bricks.Add(new TooltipBrickSpace());
				bricks.Add(new TooltipBrickText(text2, TooltipTextType.Italic));
			}
			if (!string.IsNullOrEmpty(text3))
			{
				text3 = TooltipTemplateUtils.AggregateDescription(text3, additionalDescription);
				bricks.Add(new TooltipBrickSpace());
				bricks.Add(new TooltipBrickText(text3, TooltipTextType.Paragraph));
			}
			else if (!string.IsNullOrEmpty(text))
			{
				text = TooltipTemplateUtils.AggregateDescription(text, additionalDescription);
				bricks.Add(new TooltipBrickSpace());
				bricks.Add(new TooltipBrickText(text, TooltipTextType.Paragraph));
			}
			else if (additionalDescription.Count > 0)
			{
				string text5 = TooltipTemplateUtils.AggregateDescription("", additionalDescription);
				bricks.Add(new TooltipBrickSpace());
				bricks.Add(new TooltipBrickText(text5, TooltipTextType.Paragraph));
			}
			if (!string.IsNullOrEmpty(text4))
			{
				bricks.Add(new TooltipBrickSpace());
				bricks.Add(new TooltipBrickText(text4, TooltipTextType.Paragraph));
			}
			break;
		}
		ItemEntity item = Item;
		if (item != null && item.HasUniqueSourceDescription)
		{
			bricks.Add(new TooltipBrickText(Item?.UniqueSourceDescription ?? "", TooltipTextType.Italic));
		}
	}

	private string GetEndTurn(BlueprintItem blueprintItem)
	{
		if (!((blueprintItem?.GetComponent<AddFactToEquipmentWielder>())?.Fact is BlueprintAbility blueprint))
		{
			return string.Empty;
		}
		WarhammerEndTurn component = blueprint.GetComponent<WarhammerEndTurn>();
		if (component != null)
		{
			return component.clearMPInsteadOfEndingTurn ? UIStrings.Instance.Tooltips.SpendAllMovementPointsShort : UIStrings.Instance.Tooltips.EndsTurn;
		}
		return string.Empty;
	}

	private string GetAttackAbilityGroupCooldown(BlueprintItem blueprintItem)
	{
		if (!((blueprintItem?.GetComponent<AddFactToEquipmentWielder>())?.Fact is BlueprintAbility blueprintAbility))
		{
			return string.Empty;
		}
		bool flag = false;
		foreach (BlueprintAbilityGroup abilityGroup in blueprintAbility.AbilityGroups)
		{
			if (abilityGroup.NameSafe() == "WeaponAttackAbilityGroup")
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return string.Empty;
		}
		return UIStrings.Instance.Tooltips.AttackAbilityGroupCooldownShort;
	}
}
