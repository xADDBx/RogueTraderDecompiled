using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Utility;
using Kingmaker.Controllers.Dialog;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.UI;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Kingmaker.UI.Common;

public static class UIUtilityTexts
{
	public static readonly TooltipStringsFormat TooltipString = new TooltipStringsFormat();

	private static readonly string s_ColorDefault = ColorUtility.ToHtmlStringRGB(BlueprintRoot.Instance.UIConfig.TooltipColors.Default);

	private static readonly string s_ColorBonus = ColorUtility.ToHtmlStringRGB(BlueprintRoot.Instance.UIConfig.TooltipColors.Bonus);

	private static readonly string s_ColorPenaty = ColorUtility.ToHtmlStringRGB(BlueprintRoot.Instance.UIConfig.TooltipColors.Penaty);

	public static readonly string ForceTextSymbol = "<sprite name=\"Force\">";

	public static string DirectTextSymbol = "<sprite name=\"Direct\">";

	public static string EnhancementTextSymbol = "<sprite name=\"Enhancement\">";

	public static readonly char NewLineChar = '\n';

	private static UITooltips UITooltips => LocalizedTexts.Instance.UserInterfacesText.Tooltips;

	public static string GetSkillCheckResultText(SkillCheckResult result)
	{
		string arg;
		string arg2;
		if (result.Passed)
		{
			arg = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Dialog.Succeeded;
			arg2 = s_ColorBonus;
		}
		else
		{
			arg = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Dialog.Failed;
			arg2 = s_ColorPenaty;
		}
		return string.Format(TooltipString.CentredColorText, arg, arg2);
	}

	public static string GetSkillCheckName(StatType statType)
	{
		return LocalizedTexts.Instance.Stats.GetText(statType);
	}

	public static string GetSkillCheckName(SkillCheckResult result)
	{
		return LocalizedTexts.Instance.Stats.GetText(result.StatType);
	}

	public static int GetSkillCheckChance(SkillCheckDC skillCheckDC)
	{
		if (skillCheckDC == null)
		{
			return 0;
		}
		return Mathf.Clamp(skillCheckDC.ConditionDC + skillCheckDC.ValueDC, 0, 100);
	}

	public static int GetSkillCheckChance(SkillCheckResult skillCheckDC)
	{
		if (skillCheckDC == null)
		{
			return 0;
		}
		return Mathf.Clamp(skillCheckDC.TotalSkill, 0, 100);
	}

	public static string GetSkillCheckThrow(SkillCheckResult result)
	{
		UISkillcheckTooltip skillcheckTooltips = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.SkillcheckTooltips;
		return string.Concat(string.Concat(string.Empty + string.Format(TooltipString.SimpleParameter, skillcheckTooltips.RollResult, result.RollResult), string.Format(TooltipString.ComponentParameter, skillcheckTooltips.RollD100, result.D100)), string.Format(TooltipString.ComponentParameter, skillcheckTooltips.SkillValue, UIConstsExtensions.GetValueWithSign(result.StatValue)));
	}

	public static string GetSkillCheckDC(SkillCheckResult result)
	{
		UISkillcheckTooltip skillcheckTooltips = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.SkillcheckTooltips;
		return string.Format(TooltipString.SimpleParameter, skillcheckTooltips.DifficultyClass, result.DC);
	}

	public static string GetSkillCheck(List<SkillCheckResult> skillCheckData)
	{
		PFLog.UI.Log("SkillCheckData.Count " + skillCheckData.Count);
		if (skillCheckData.Count <= 0)
		{
			return null;
		}
		UISkillcheckTooltip skillcheckTooltips = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.SkillcheckTooltips;
		string text = string.Empty;
		foreach (SkillCheckResult skillCheckDatum in skillCheckData)
		{
			string arg;
			string arg2;
			if (skillCheckDatum.Passed)
			{
				arg = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Dialog.Succeeded;
				arg2 = s_ColorBonus;
			}
			else
			{
				arg = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Dialog.Failed;
				arg2 = s_ColorPenaty;
			}
			text += string.Format(TooltipString.H3, skillcheckTooltips.SkillCheck, s_ColorDefault);
			text += string.Format(TooltipString.SimpleParameter, skillcheckTooltips.RollD100, skillCheckDatum.RollResult);
			text += string.Format(TooltipString.SimpleParameter, skillcheckTooltips.SkillValue, skillCheckDatum.StatValue);
			text += string.Format(TooltipString.CentredText, skillcheckTooltips.Against);
			text += string.Format(TooltipString.SimpleParameter, skillcheckTooltips.DC, skillCheckDatum.DC);
			text += string.Format(TooltipString.H3, skillcheckTooltips.Result, s_ColorDefault);
			text += string.Format(TooltipString.H2, arg, arg2);
		}
		return text;
	}

	public static string GetLootSkillCheck(SkillCheckResult skillCheckResult)
	{
		if (skillCheckResult == null)
		{
			return string.Empty;
		}
		StringBuilder builder = ContextData<PooledStringBuilder>.Request().Builder;
		UILoot lootWindow = UIStrings.Instance.LootWindow;
		builder.Append(string.Format(arg0: LocalizedTexts.Instance.Stats.GetText(skillCheckResult.StatType), format: lootWindow.SkillCheckTitle.Text));
		builder.Append(NewLineChar);
		builder.Append(string.Format(arg0: (string)(skillCheckResult.Passed ? UIStrings.Instance.Dialog.Succeeded : UIStrings.Instance.Dialog.Failed), format: lootWindow.SkillCheckResult.Text));
		builder.Append(NewLineChar);
		builder.Append(string.Format(lootWindow.SkillCheckValueAgainst.Text, skillCheckResult.RollResult, skillCheckResult.DC));
		builder.Append(NewLineChar);
		builder.Append(string.Format(lootWindow.SkillCheckSkillValue.Text, skillCheckResult.TotalSkill));
		return builder.ToString();
	}

	public static string GetItemWeight(ItemEntity item)
	{
		float weight = item.Blueprint.Weight;
		if (item.Count == 1)
		{
			return GetItemWeight(weight);
		}
		float num = weight * (float)item.Count;
		return $"{weight:#####0.##} ({num:#####0.##}) {UIStrings.Instance.Tooltips.lbs}";
	}

	public static string GetItemWeight(BlueprintItem blueprintItem)
	{
		return GetItemWeight(SimpleBlueprintExtendAsObject.Or(blueprintItem, null)?.Weight ?? 0f);
	}

	public static string GetItemWeight(float weight)
	{
		return $"{weight:#####0.##} {UIStrings.Instance.Tooltips.lbs}";
	}

	private static string GetEquipPosibilityString(string text, Color32 color)
	{
		return string.Format(TooltipString.EquipPosibility, text, ColorUtility.ToHtmlStringRGB(color));
	}

	public static string FormatFailReason(string reason)
	{
		string arg = ColorUtility.ToHtmlStringRGB(BlueprintRoot.Instance.UIConfig.TooltipColors.CanNotEquip);
		return string.Format(TooltipString.CentredColorText, reason, arg);
	}

	public static string GetAPCostValue(string val)
	{
		string arg = ColorUtility.ToHtmlStringRGB(BlueprintRoot.Instance.UIConfig.TooltipColors.TooltipValue);
		return string.Format(val, arg);
	}

	public static string GetPortraitsPath()
	{
		return CustomPortraitsManager.Instance.GetCurrentPortraitFolderPath().Replace("/", "\\");
	}

	public static string GetAbilityTarget(BlueprintAbility blueprintAbility, BlueprintItem blueprintItem)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			ItemEntityWeapon overrideWeapon = null;
			if (blueprintItem != null)
			{
				overrideWeapon = blueprintItem.CreateEntity() as ItemEntityWeapon;
			}
			BaseUnitEntity caster = UIUtility.GetCurrentSelectedUnit() ?? Game.Instance.DefaultUnit;
			return GetAbilityTarget(new AbilityData(blueprintAbility, caster)
			{
				OverrideWeapon = overrideWeapon
			});
		}
	}

	public static string GetAbilityTarget(AbilityData abilitydata)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			if (abilitydata.SourceItemUsableBlueprint != null && abilitydata.SourceItemUsableBlueprint.Type == UsableItemType.Potion)
			{
				return LocalizedTexts.Instance.AbilityTargets.Personal;
			}
			if (abilitydata.Blueprint.Range == AbilityRange.Weapon && abilitydata.Weapon != null)
			{
				return abilitydata.Blueprint.GetTarget(abilitydata.Weapon.AttackRange, abilitydata.Caster);
			}
			return abilitydata.Blueprint.GetTarget(-1, abilitydata.Caster);
		}
	}

	public static Sprite GetTargetImage(Ability ability)
	{
		return GetTargetImage(ability.Blueprint);
	}

	public static Sprite GetTargetImage(BlueprintAbility blueprintAbility)
	{
		return blueprintAbility.GetTargetImage();
	}

	public static string GetTextByKey(DamageType typeDescriptionEnergy)
	{
		LocalizedString localizedString = Enumerable.FirstOrDefault(BlueprintRoot.Instance.LocalizedTexts.DamageTypes.Entries, (DamageTypeStrings.MyEntry entry) => entry.Value == typeDescriptionEnergy)?.Text;
		if (localizedString == null)
		{
			return string.Empty;
		}
		return localizedString;
	}

	public static string GetSpellDescriptorsText(BlueprintAbility abilityBlueprint)
	{
		if (abilityBlueprint != null)
		{
			return GetSpellDescriptor(abilityBlueprint.SpellDescriptor);
		}
		return string.Empty;
	}

	public static string GetLongOrShortText(string description, bool state)
	{
		string text = description;
		if (state)
		{
			if (text.Contains("[LONGSTART]"))
			{
				while (text.Contains("[LONGSTART]"))
				{
					int startIndex = text.IndexOf("[LONGSTART]", StringComparison.Ordinal);
					int startIndex2 = text.IndexOf("[LONGSTART]", StringComparison.Ordinal) + 11;
					string text2 = text.Substring(startIndex2);
					text = text.Remove(startIndex) + text2;
				}
			}
			if (text.Contains("[LONGEND]"))
			{
				while (text.Contains("[LONGEND]"))
				{
					int startIndex3 = text.IndexOf("[LONGEND]", StringComparison.Ordinal);
					int startIndex4 = text.IndexOf("[LONGEND]", StringComparison.Ordinal) + 9;
					string text3 = text.Substring(startIndex4);
					text = text.Remove(startIndex3) + text3;
				}
			}
		}
		else if (text.Contains("[LONGSTART]") && text.Contains("[LONGEND]"))
		{
			while (text.Contains("[LONGSTART]"))
			{
				int startIndex5 = text.IndexOf("[LONGSTART]", StringComparison.Ordinal);
				int num = text.IndexOf("[LONGEND]", StringComparison.Ordinal);
				string text4 = text.Remove(startIndex5);
				if (num < 0)
				{
					text = text4;
					continue;
				}
				int num2 = num + 9;
				string text5 = text;
				int num3 = num2;
				string text6 = text5.Substring(num3, text5.Length - num3);
				text = text4 + text6;
			}
		}
		return text;
	}

	public static string GetAbilityActionText(BlueprintAbility blueprint, BlueprintItemEquipmentUsable item = null)
	{
		if (blueprint != null)
		{
			return "<obsolete>";
		}
		return string.Empty;
	}

	public static string GetAbilityActionText(AbilityData dataAbilityData)
	{
		if (!(dataAbilityData == null))
		{
			return "<obsolete>";
		}
		return string.Empty;
	}

	public static string GetSpellDescriptor(SpellDescriptor spellDescriptor)
	{
		return BlueprintRoot.Instance.LocalizedTexts.SpellDescriptorNames.GetText(spellDescriptor);
	}

	public static string GetConditionText(UnitCondition condition)
	{
		LocalizedString localizedString = Enumerable.FirstOrDefault(BlueprintRoot.Instance.LocalizedTexts.UnitConditions.Entries, (ConditionsString.MyEntry entry) => entry.Value == condition)?.Text;
		if (localizedString == null)
		{
			return string.Empty;
		}
		return localizedString;
	}

	public static string GetBlueprintUnitFactNameText(BlueprintUnitFact blueprintUnitFact)
	{
		if (blueprintUnitFact == null)
		{
			return string.Empty;
		}
		return blueprintUnitFact.Name;
	}

	public static void TryAddWordSeparator(StringBuilder description, string conjunction)
	{
		if (description.Length > 0)
		{
			description.Append(" " + conjunction + " ");
		}
	}

	public static void AddWordWithComma(StringBuilder stringBuilder, string word)
	{
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Append(", ");
		}
		stringBuilder.Append(word);
	}

	public static string FirstCharToUpper(string input)
	{
		if (!string.IsNullOrEmpty(input))
		{
			return input.First().ToString().ToUpper() + input.Substring(1, input.Length - 1);
		}
		return input;
	}

	public static string GetDamageSymbols(DamageTypeDescription damageTypeDescription)
	{
		_ = string.Empty;
		return ForceTextSymbol;
	}

	public static string GetDamageNames(DamageTypeDescription damageTypeDescription)
	{
		_ = string.Empty;
		return UIUtility.GetGlossaryEntryName(damageTypeDescription.Type.ToString());
	}

	public static string GetStatShortName(StatType statType)
	{
		UIStrings userInterfacesText = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText;
		return statType switch
		{
			StatType.WarhammerWeaponSkill => userInterfacesText.CharacterSheet.WeaponSkillShort.Text, 
			StatType.WarhammerBallisticSkill => userInterfacesText.CharacterSheet.BallisticSkillShort.Text, 
			StatType.WarhammerStrength => userInterfacesText.CharacterSheet.StrengthShort.Text, 
			StatType.WarhammerToughness => userInterfacesText.CharacterSheet.ToughnessShort.Text, 
			StatType.WarhammerAgility => userInterfacesText.CharacterSheet.AgilityShort.Text, 
			StatType.WarhammerIntelligence => userInterfacesText.CharacterSheet.InteligenceShort.Text, 
			StatType.WarhammerPerception => userInterfacesText.CharacterSheet.PerceptionShort.Text, 
			StatType.WarhammerWillpower => userInterfacesText.CharacterSheet.WillpowerShort.Text, 
			StatType.WarhammerFellowship => userInterfacesText.CharacterSheet.FellowshipShort.Text, 
			_ => string.Empty, 
		};
	}

	public static string UpdateDescriptionWithUIProperties(string description, MechanicEntity calculationSource, bool selectedUnitCalculateInInventory = false)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			int num = 0;
			string text = string.Empty;
			try
			{
				while (num < description.Length)
				{
					int num2 = description.IndexOf("{" + EntityLink.GetTag(EntityLink.Type.UIProperty) + "|", num);
					if (num2 == -1)
					{
						text += description.Substring(num);
						break;
					}
					_ = description[num2];
					text += description.Substring(num, num2 - num);
					num = num2;
					num2 += 5;
					int num3 = description.Substring(num2).IndexOf('|');
					int num4 = description.Substring(num2).IndexOf('}');
					if (num3 == -1 || num4 == -1 || num3 > num4)
					{
						return description;
					}
					num2 += num3;
					string link = description.Substring(num + 5, num2 - num - 5);
					num = num2 + 1;
					num2 += description.Substring(num2).IndexOf('}');
					string assetId = description.Substring(num, num2 - num);
					num = num2 + 1;
					BlueprintUnitFact blueprintUnitFact = ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>(assetId);
					if (calculationSource != null)
					{
						Ability ability = calculationSource.Facts.Get<Ability>(blueprintUnitFact);
						UnitFact unitFact = null;
						if (ability == null)
						{
							unitFact = calculationSource.Facts.Get<UnitFact>(blueprintUnitFact);
						}
						UIPropertySettings property = ((ability != null) ? ability.GetComponent<UIPropertiesComponent>() : unitFact?.GetComponent<UIPropertiesComponent>())?.Properties.FirstOrDefault((UIPropertySettings property) => property.LinkKey == link);
						if (property != null)
						{
							int? num5 = (property.PropertySource ?? ((ability != null) ? ability.Blueprint : unitFact.Blueprint)).GetComponents<PropertyCalculatorComponent>().FirstOrDefault((PropertyCalculatorComponent c) => c.Name == property.PropertyName)?.GetValue(new PropertyContext(calculationSource, (ability == null) ? ((MechanicEntityFact)unitFact) : ((MechanicEntityFact)ability)));
							string glossaryMechanicsHTML = UIConfig.Instance.PaperGlossaryColors.GlossaryMechanicsHTML;
							string text2 = ((num5 == 0) ? (" [" + property.Description.Text + "]") : string.Empty);
							if (num5.HasValue)
							{
								link = $"<b><color={glossaryMechanicsHTML}><link=\"{EntityLink.GetTag(EntityLink.Type.UIProperty)}:{blueprintUnitFact.AssetGuid}:{link}\">{Mathf.Abs(num5.Value)}</link></color></b>{text2}";
							}
						}
						else
						{
							property = blueprintUnitFact.GetComponent<UIPropertiesComponent>()?.Properties.FirstOrDefault((UIPropertySettings p) => p.LinkKey == link);
							if (property != null)
							{
								if (!selectedUnitCalculateInInventory)
								{
									link = FormatIndent(property.Description);
								}
								else
								{
									int? num6 = (property.PropertySource ?? ((ability != null) ? ability.Blueprint : unitFact.Blueprint)).GetComponents<PropertyCalculatorComponent>().FirstOrDefault((PropertyCalculatorComponent c) => c.Name == property.PropertyName)?.GetValue(new PropertyContext(calculationSource, (ability == null) ? ((MechanicEntityFact)unitFact) : ((MechanicEntityFact)ability)));
									string text3 = ((num6 == 0) ? (" [" + property.Description.Text + "]") : string.Empty);
									string glossaryMechanicsHTML2 = UIConfig.Instance.PaperGlossaryColors.GlossaryMechanicsHTML;
									if (num6.HasValue)
									{
										link = $"<b><color={glossaryMechanicsHTML2}><link=\"{EntityLink.GetTag(EntityLink.Type.UIProperty)}:{blueprintUnitFact.AssetGuid}:{link}\">{Mathf.Abs(num6.Value)}</link></color></b>{text3}";
									}
								}
							}
						}
					}
					else
					{
						UIPropertySettings uIPropertySettings = blueprintUnitFact.GetComponent<UIPropertiesComponent>()?.Properties.FirstOrDefault((UIPropertySettings property) => property.LinkKey == link);
						if (uIPropertySettings != null)
						{
							link = FormatIndent(uIPropertySettings.Description);
						}
					}
					text += link;
				}
			}
			catch (Exception arg)
			{
				PFLog.UI.Error($"{arg}");
			}
			return text;
		}
	}

	private static string FormatIndent(string text)
	{
		if (text.StartsWith("<indent=") || text.StartsWith("-<indent="))
		{
			text = "\n" + text;
		}
		return text;
	}

	public static (string, ItemHeaderType) GetItemHeaderText(ItemEntity item)
	{
		BaseUnitEntity currentSelectedUnit = UIUtility.GetCurrentSelectedUnit();
		if (item == null || currentSelectedUnit == null)
		{
			return (string.Empty, ItemHeaderType.Header);
		}
		string itemOwnerName = UIUtilityItem.GetItemOwnerName(item);
		if (item is ItemEntityWeapon || item is ItemEntityArmor || item is ItemEntityShield || item.Blueprint is BlueprintItemEquipment)
		{
			if (item.Owner == null)
			{
				if (currentSelectedUnit.IsPet)
				{
					if (item.Blueprint is BlueprintItemEquipmentPetProtocol && !item.CanBeEquippedBy(currentSelectedUnit))
					{
						LocalizedString localizedString = UITooltips.PetCanNotEquip;
						EquipmentRestrictionMasterHasFacts component = item.Blueprint.GetComponent<EquipmentRestrictionMasterHasFacts>();
						if (component != null && !component.CanBeEquippedBy(currentSelectedUnit))
						{
							BlueprintUnitFact blueprintUnitFact = component.Facts.FirstOrDefault((BlueprintUnitFact f) => f is BlueprintSoulMark);
							if (blueprintUnitFact == null || blueprintUnitFact.NameForAcronym == null)
							{
								return (localizedString, ItemHeaderType.CanNotEquip);
							}
							if (blueprintUnitFact.NameForAcronym.Contains("Faith"))
							{
								localizedString = UITooltips.PetCanNotEquipOverseerDogmatic;
							}
							else if (blueprintUnitFact.NameForAcronym.Contains("Hope"))
							{
								localizedString = UITooltips.PetCanNotEquipOverseerSchismatic;
							}
							else if (blueprintUnitFact.NameForAcronym.Contains("Corruption"))
							{
								localizedString = UITooltips.PetCanNotEquipOverseerHeretic;
							}
						}
						return (localizedString, ItemHeaderType.CanNotEquip);
					}
					if (item.Blueprint is BlueprintItemEquipmentPetProtocol && item.CanBeEquippedBy(currentSelectedUnit))
					{
						return (UITooltips.CanBeEquip, ItemHeaderType.Header);
					}
					return (UITooltips.PetCanNotEquip, ItemHeaderType.CanNotEquip);
				}
				if (item.Blueprint is BlueprintItemEquipmentPetProtocol)
				{
					return (UITooltips.OnlyPetCanEquip, ItemHeaderType.Header);
				}
				if (item.CanBeEquippedBy(currentSelectedUnit))
				{
					return (UITooltips.CanBeEquip, ItemHeaderType.Header);
				}
				return (UITooltips.CannotbeEquip, ItemHeaderType.CanNotEquip);
			}
			if (UIUtility.GetGroup().Contains(item.Owner))
			{
				return (itemOwnerName, ItemHeaderType.Equipped);
			}
		}
		else if (item is ItemEntityUsable itemEntityUsable)
		{
			bool flag = UIUtilityItem.IsItemAbilityInSpellListOfUnit(itemEntityUsable.Blueprint, currentSelectedUnit);
			if (!(itemEntityUsable.Blueprint.Abilities.FirstOrDefault() == null || flag))
			{
				UsableItemType type = itemEntityUsable.Blueprint.Type;
				if (type != UsableItemType.Potion && type != 0)
				{
					return (UITooltips.CannotbeUsed, ItemHeaderType.CanNotEquip);
				}
			}
			return (UITooltips.CanBeUsed, ItemHeaderType.Header);
		}
		return (string.Empty, ItemHeaderType.Header);
	}

	public static string GetPercentString(float value)
	{
		if (!(value >= 0f))
		{
			return "";
		}
		return $"{Math.Round(value, 1)}%";
	}

	public static string WrapWithWeight(string text, TextFontWeight weight)
	{
		return weight switch
		{
			TextFontWeight.Thin => "<font-weight=\"100\">" + text + "</font-weight>", 
			TextFontWeight.ExtraLight => "<font-weight=\"200\">" + text + "</font-weight>", 
			TextFontWeight.Light => "<font-weight=\"300\">" + text + "</font-weight>", 
			TextFontWeight.Regular => "<font-weight=\"400\">" + text + "</font-weight>", 
			TextFontWeight.Medium => "<font-weight=\"500\">" + text + "</font-weight>", 
			TextFontWeight.SemiBold => "<font-weight=\"600\">" + text + "</font-weight>", 
			TextFontWeight.Bold => "<font-weight=\"700\">" + text + "</font-weight>", 
			TextFontWeight.Heavy => "<font-weight=\"800\">" + text + "</font-weight>", 
			TextFontWeight.Black => "<font-weight=\"900\">" + text + "</font-weight>", 
			_ => text, 
		};
	}
}
