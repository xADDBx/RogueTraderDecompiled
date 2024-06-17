using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks.Utils;
using Kingmaker.Controllers.Enums;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.Models.Tooltip;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.UI;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.Utility.UnitDescription;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateAbility : TooltipBaseTemplate
{
	public readonly AbilityData AbilityData;

	public readonly BlueprintAbility BlueprintAbility;

	public readonly BlueprintItem SourceItem;

	private readonly MechanicEntity m_Caster;

	private string m_Name = string.Empty;

	private Sprite m_Icon;

	private string m_Type = string.Empty;

	private string m_Cost = string.Empty;

	private readonly string m_Level = string.Empty;

	private string m_Veil = string.Empty;

	private string m_Target = string.Empty;

	private Sprite m_TargetIcon;

	private string m_Duration = string.Empty;

	private string m_Cooldown = string.Empty;

	private string m_EndTurn = string.Empty;

	private string m_AttackAbilityGroupCooldown = string.Empty;

	private string m_SavingThrow = string.Empty;

	private string m_ShortDescriptionText = string.Empty;

	private string m_LongDescriptionText = string.Empty;

	private string m_SpellDescriptor = string.Empty;

	private string m_ActionTime = string.Empty;

	private readonly UnitDescription.UIDamageInfo[] m_DamageInfo;

	private readonly string m_AutoCastHint = string.Empty;

	private UIUtilityItem.UIAbilityData m_UIAbilityData;

	private bool m_IsReload;

	private readonly ItemEntityWeapon m_Weapon;

	private bool m_IsOnTimeInBattleAbility;

	private bool IsWeaponAbility
	{
		get
		{
			BlueprintItem sourceItem = SourceItem;
			return sourceItem is BlueprintItemWeapon || sourceItem is BlueprintStarshipItem;
		}
	}

	private bool IsSpaceCombatAbility => Game.Instance.CurrentMode == GameModeType.SpaceCombat;

	public override void Prepare(TooltipTemplateType type)
	{
		if (AbilityData != null)
		{
			FillAbilityDataInfo(AbilityData);
		}
		else if (BlueprintAbility != null)
		{
			FillBlueprintAbilityData(BlueprintAbility);
		}
	}

	public TooltipTemplateAbility(BlueprintAbility blueprintAbility, BlueprintItem sourceItem = null, MechanicEntity caster = null)
	{
		BlueprintAbility = blueprintAbility;
		SourceItem = sourceItem;
		m_Caster = caster;
	}

	public TooltipTemplateAbility(AbilityData abilityData)
	{
		AbilityData = abilityData;
		m_DamageInfo = null;
		BlueprintAbility = abilityData.Blueprint;
		m_Caster = abilityData.Caster;
		SourceItem = abilityData.SourceItem?.Blueprint;
		m_Weapon = abilityData.SourceItem as ItemEntityWeapon;
	}

	private void FillBlueprintAbilityData(BlueprintAbility blueprintAbility)
	{
		try
		{
			BlueprintItemWeapon blueprintItem = SourceItem as BlueprintItemWeapon;
			m_Name = blueprintAbility.Name;
			m_Icon = blueprintAbility.Icon;
			m_Type = GetAbilityType(blueprintAbility);
			m_Target = UIUtilityTexts.GetAbilityTarget(blueprintAbility, blueprintItem);
			m_TargetIcon = UIUtilityTexts.GetTargetImage(blueprintAbility);
			m_Duration = blueprintAbility.LocalizedDuration;
			m_Cooldown = blueprintAbility.CooldownRounds.ToString();
			m_IsOnTimeInBattleAbility = CheckOneTimeInBattleAbility(blueprintAbility);
			m_EndTurn = GetEndTurn(blueprintAbility);
			m_AttackAbilityGroupCooldown = GetAttackAbilityGroupCooldown(blueprintAbility);
			m_SavingThrow = blueprintAbility.LocalizedSavingThrow;
			m_ShortDescriptionText = blueprintAbility.GetShortenedDescription();
			m_LongDescriptionText = blueprintAbility.Description;
			m_SpellDescriptor = UIUtilityTexts.GetSpellDescriptorsText(blueprintAbility);
			m_ActionTime = UIUtilityTexts.GetAbilityActionText(blueprintAbility);
			m_UIAbilityData = UIUtilityItem.GetUIAbilityData(blueprintAbility, blueprintItem, m_Caster);
			m_IsReload = UIUtilityItem.IsReload(blueprintAbility);
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {blueprintAbility.name}: {arg}");
		}
	}

	private void FillAbilityDataInfo(AbilityData abilityData)
	{
		try
		{
			using (GameLogContext.Scope)
			{
				GameLogContext.UnitEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)m_Caster;
				m_Name = abilityData.Name;
				m_Icon = abilityData.Icon;
				m_Type = GetAbilityType(abilityData.Blueprint);
				m_Cost = GetCost(abilityData);
				m_Veil = GetVeil(abilityData);
				m_EndTurn = GetEndTurn(abilityData.Blueprint);
				m_AttackAbilityGroupCooldown = GetAttackAbilityGroupCooldown(abilityData.Blueprint);
				m_Target = UIUtilityTexts.GetAbilityTarget(abilityData);
				m_TargetIcon = UIUtilityTexts.GetTargetImage(abilityData.Blueprint);
				m_Duration = abilityData.Blueprint.LocalizedDuration;
				m_Cooldown = abilityData.Blueprint.CooldownRounds.ToString();
				m_IsOnTimeInBattleAbility = CheckOneTimeInBattleAbility(abilityData.Blueprint);
				m_SavingThrow = abilityData.Blueprint.LocalizedSavingThrow;
				m_ShortDescriptionText = abilityData.ShortenedDescription;
				m_LongDescriptionText = abilityData.Description;
				m_SpellDescriptor = UIUtilityTexts.GetSpellDescriptorsText(abilityData.Blueprint);
				m_ActionTime = UIUtilityTexts.GetAbilityActionText(abilityData);
				m_UIAbilityData = UIUtilityItem.GetUIAbilityData(abilityData.Blueprint, abilityData.Weapon);
				m_IsReload = UIUtilityItem.IsReload(abilityData);
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {abilityData?.Blueprint?.name}: {arg}");
		}
	}

	private bool CheckOneTimeInBattleAbility(BlueprintAbility blueprintAbility)
	{
		return blueprintAbility.GetComponent<WarhammerCooldown>()?.UntilEndOfCombat ?? false;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return AddAbilityHeader();
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddDamageInfo(list);
		AddTarget(list);
		AddDuration(list);
		AddCooldown(list);
		AddHitChances(list);
		AddDescription(list, type);
		AddMovementActionVeil(list, type);
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		if (type == TooltipTemplateType.Tooltip && !string.IsNullOrEmpty(m_AutoCastHint))
		{
			yield return new TooltipBrickSeparator(TooltipBrickElementType.Medium);
			yield return new TooltipBrickText(m_AutoCastHint, TooltipTextType.Italic);
		}
	}

	private ITooltipBrick AddAbilityHeader()
	{
		TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
		{
			Text = m_UIAbilityData.Name,
			TextParams = new TextFieldParams
			{
				FontStyles = TMPro.FontStyles.Bold
			}
		};
		TooltipBrickIconPattern.TextFieldValues textFieldValues = new TooltipBrickIconPattern.TextFieldValues
		{
			TextParams = new TextFieldParams()
		};
		TooltipBrickIconPattern.TextFieldValues tertiaryValues = null;
		if (m_UIAbilityData.BurstAttacksCount > 1)
		{
			tertiaryValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = string.Format(UIStrings.Instance.Tooltips.ShotsCount, m_UIAbilityData.BurstAttacksCount.ToString()),
				TextParams = new TextFieldParams
				{
					FontColor = Color.black
				}
			};
		}
		if (m_IsReload)
		{
			if (m_Weapon != null)
			{
				int currentAmmo = m_Weapon.CurrentAmmo;
				int warhammerMaxAmmo = m_Weapon.Blueprint.WarhammerMaxAmmo;
				tertiaryValues = new TooltipBrickIconPattern.TextFieldValues
				{
					Text = string.Format(UIStrings.Instance.Tooltips.ShotsCount, $"{currentAmmo}/{warhammerMaxAmmo}"),
					TextParams = new TextFieldParams
					{
						FontColor = Color.black
					}
				};
			}
			else
			{
				UberDebug.LogError("Error: Weapon is null");
			}
		}
		switch (m_UIAbilityData.MomentumAbilityType)
		{
		case MomentumAbilityType.HeroicAct:
			textFieldValues.Text = UIStrings.Instance.Tooltips.HeroicActAbility;
			textFieldValues.TextParams.FontColor = UIConfig.Instance.TooltipColors.HeroicActAbility;
			break;
		case MomentumAbilityType.DesperateMeasure:
			textFieldValues.Text = UIStrings.Instance.Tooltips.DesperateMeasureAbility;
			textFieldValues.TextParams.FontColor = UIConfig.Instance.TooltipColors.DesperateMeasureAbility;
			break;
		default:
			textFieldValues.Text = ((!IsSpaceCombatAbility) ? m_UIAbilityData.CostAP : string.Empty);
			textFieldValues.TextParams.FontColor = Color.black;
			break;
		}
		return new TooltipBrickIconPattern(m_UIAbilityData.Icon, null, titleValues, textFieldValues, tertiaryValues);
	}

	private string GetCost(BlueprintAbility blueprintAbility)
	{
		int actionPointCost = blueprintAbility.ActionPointCost;
		return GetCost(actionPointCost, blueprintAbility);
	}

	private string GetCost(AbilityData abilityData)
	{
		int cost = abilityData.CalculateActionPointCost();
		return GetCost(cost, abilityData.Blueprint);
	}

	private string GetVeil(AbilityData abilityData)
	{
		if (abilityData.Blueprint.AbilityParamsSource != WarhammerAbilityParamsSource.PsychicPower)
		{
			return string.Empty;
		}
		if (abilityData.Blueprint.PsychicPower != PsychicPower.Major)
		{
			return UIStrings.Instance.Tooltips.MinorVeilDegradation.Text;
		}
		return UIStrings.Instance.Tooltips.MajorVeilDegradation.Text;
	}

	private string GetCost(int cost, BlueprintAbility blueprintAbility)
	{
		if (blueprintAbility.AbilityParamsSource != WarhammerAbilityParamsSource.PsychicPower)
		{
			return string.Format(UIStrings.Instance.Tooltips.CostAP, cost);
		}
		return string.Format(UIStrings.Instance.Tooltips.PsychicPowerCostAP, cost, blueprintAbility.GetVeilThicknessPointsToAdd());
	}

	private string GetAbilityType(BlueprintAbility blueprintAbility)
	{
		if (blueprintAbility.AbilityParamsSource != WarhammerAbilityParamsSource.PsychicPower)
		{
			return LocalizedTexts.Instance.AbilityTypes.GetText(blueprintAbility.Type);
		}
		return UIStrings.Instance.Tooltips.PsykerPower;
	}

	private string GetEndTurn(BlueprintAbility blueprintAbility)
	{
		WarhammerEndTurn component = blueprintAbility.GetComponent<WarhammerEndTurn>();
		if (component != null)
		{
			return component.clearMPInsteadOfEndingTurn ? UIStrings.Instance.Tooltips.SpendAllMovementPoints : UIStrings.Instance.Tooltips.EndsTurn;
		}
		return string.Empty;
	}

	private string GetAttackAbilityGroupCooldown(BlueprintAbility blueprintAbility)
	{
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
		return UIStrings.Instance.Tooltips.AttackAbilityGroupCooldown;
	}

	private void AddTarget(List<ITooltipBrick> bricks)
	{
		if (!m_IsReload && !string.IsNullOrEmpty(m_Target))
		{
			TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = string.Empty
			};
			TooltipBrickIconPattern.TextFieldValues secondaryValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = m_Target
			};
			bricks.Add(new TooltipBrickIconPattern(m_TargetIcon, m_UIAbilityData.PatternData, titleValues, secondaryValues));
		}
	}

	private void AddDuration(List<ITooltipBrick> bricks)
	{
		if (!string.IsNullOrEmpty(m_Duration))
		{
			TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = UIStrings.Instance.TooltipsElementLabels.GetLabel(TooltipElement.Duration)
			};
			TooltipBrickIconPattern.TextFieldValues secondaryValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = UIUtilityTexts.WrapWithWeight(m_Duration, TextFontWeight.SemiBold)
			};
			bricks.Add(new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.Duration, null, titleValues, secondaryValues));
		}
	}

	private void AddCooldown(List<ITooltipBrick> bricks)
	{
		if ((!string.IsNullOrEmpty(m_Cooldown) && !(m_Cooldown == "0")) || m_IsOnTimeInBattleAbility)
		{
			TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = UIStrings.Instance.TooltipsElementLabels.GetLabel(TooltipElement.Cooldown)
			};
			TooltipBrickIconPattern.TextFieldValues secondaryValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = UIUtilityTexts.WrapWithWeight((!m_IsOnTimeInBattleAbility) ? string.Concat(UIStrings.Instance.TurnBasedTexts.Rounds, ": ", m_Cooldown) : ((string)UIStrings.Instance.TurnBasedTexts.CanUseOneTimeInCombat), TextFontWeight.SemiBold)
			};
			bricks.Add(new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.Cooldown, null, titleValues, secondaryValues, null, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0)));
		}
	}

	private void AddHitChances(List<ITooltipBrick> bricks)
	{
		if (m_IsReload || !IsWeaponAbility || IsSpaceCombatAbility)
		{
			return;
		}
		if (m_UIAbilityData.IsRange)
		{
			if (m_UIAbilityData.IsScatter)
			{
				AddScatterHitChances(bricks);
			}
			else
			{
				AddRangeHitChances(bricks);
			}
		}
		else
		{
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Tooltips.HitChances, $"{m_UIAbilityData.HitChance}%"));
		}
	}

	private void AddRangeHitChances(List<ITooltipBrick> bricks)
	{
		TextFieldParams textFieldParams = new TextFieldParams
		{
			FontStyles = TMPro.FontStyles.Bold
		};
		TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
		{
			Text = UIStrings.Instance.Tooltips.HitChances
		};
		TooltipBrickIconPattern.TextFieldValues secondaryValues = new TooltipBrickIconPattern.TextFieldValues
		{
			Text = UIStrings.Instance.Tooltips.HitChancesEffectiveDistance,
			Value = $"{m_UIAbilityData.HitChance}%",
			TextParams = textFieldParams,
			ValueParams = textFieldParams
		};
		TooltipBrickIconPattern.TextFieldValues tertiaryValues = new TooltipBrickIconPattern.TextFieldValues
		{
			Text = UIStrings.Instance.Tooltips.HitChancesMaxDistance,
			Value = $"{m_UIAbilityData.HitChance / 2}%",
			TextParams = textFieldParams,
			ValueParams = textFieldParams
		};
		bricks.Add(new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.HitChances, null, titleValues, secondaryValues, tertiaryValues));
	}

	private void AddScatterHitChances(List<ITooltipBrick> bricks)
	{
		UIUtilityItem.UIScatterHitChanceData scatterHitChanceData = m_UIAbilityData.ScatterHitChanceData;
		UITooltips tooltips = UIStrings.Instance.Tooltips;
		bricks.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.HitChances, TooltipTitleType.H1));
		bricks.Add(new TooltipBricksGroupStart());
		bricks.Add(new TooltipBrickText(UIStrings.Instance.Tooltips.HitChancesEffectiveDistance, TooltipTextType.Bold));
		bricks.Add(new TooltipBrickIconStatValue(tooltips.ScatterMainLineClose, $"{scatterHitChanceData.MainLineClose}%"));
		bricks.Add(new TooltipBrickIconStatValue(tooltips.ScatterClose, $"{scatterHitChanceData.ScatterClose}%"));
		bricks.Add(new TooltipBricksGroupEnd());
		bricks.Add(new TooltipBricksGroupStart());
		bricks.Add(new TooltipBrickText(UIStrings.Instance.Tooltips.HitChancesMaxDistance, TooltipTextType.Bold));
		bricks.Add(new TooltipBrickIconStatValue(tooltips.ScatterMainLine, $"{scatterHitChanceData.MainLine}%"));
		bricks.Add(new TooltipBrickIconStatValue(tooltips.ScatterNear, $"{scatterHitChanceData.ScatterNear}%"));
		bricks.Add(new TooltipBrickIconStatValue(tooltips.ScatterFar, $"{scatterHitChanceData.ScatterFar}%"));
		bricks.Add(new TooltipBricksGroupEnd());
	}

	private void AddUIProperties(List<ITooltipBrick> bricks)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			if (!m_UIAbilityData.UIProperties.Any())
			{
				return;
			}
			bricks.Add(new TooltipBricksGroupStart());
			foreach (UIProperty uIProperty in m_UIAbilityData.UIProperties)
			{
				string title = ((!string.IsNullOrEmpty(uIProperty.Name)) ? uIProperty.Name : uIProperty.NameType.GetLocalizedName());
				bricks.Add(new TooltipBrickCalculatedFormula(title, uIProperty.Description, uIProperty.PropertyValue?.ToString() ?? ((string)UIStrings.Instance.SettingsUI.Value), !uIProperty.PropertyValue.HasValue));
			}
			bricks.Add(new TooltipBricksGroupEnd());
		}
	}

	private void AddDamageInfo(List<ITooltipBrick> bricks)
	{
		if (!m_IsReload)
		{
			string baseDamageText = m_UIAbilityData.BaseDamageText;
			_ = m_UIAbilityData.DamageText;
			string valueRight = m_UIAbilityData.Penetration + "%";
			if (!string.IsNullOrEmpty(baseDamageText))
			{
				Sprite damage = UIConfig.Instance.UIIcons.Damage;
				Sprite penetration = UIConfig.Instance.UIIcons.Penetration;
				string label = UIStrings.Instance.TooltipsElementLabels.GetLabel(TooltipElement.Damage);
				string label2 = UIStrings.Instance.TooltipsElementLabels.GetLabel(TooltipElement.Penetration);
				bricks.Add(new TooltipBrickTwoColumnsStat(label, label2, baseDamageText, valueRight, damage, penetration));
			}
		}
	}

	private void AddDescription(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		string description = string.Empty;
		switch (type)
		{
		case TooltipTemplateType.Tooltip:
			description = m_ShortDescriptionText;
			break;
		case TooltipTemplateType.Info:
			description = m_LongDescriptionText;
			break;
		}
		description = TooltipTemplateUtils.AggregateDescription(description, TooltipTemplateUtils.GetAdditionalDescription(BlueprintAbility));
		if (!string.IsNullOrEmpty(description))
		{
			description = UIUtilityTexts.UpdateDescriptionWithUIProperties(description, m_Caster);
			description = UpdateDescriptionWithUICommonProperties(description);
			bricks.Add(new TooltipBrickText("\n" + description + "\n\n", TooltipTextType.Paragraph));
		}
	}

	private string UpdateDescriptionWithUICommonProperties(string description)
	{
		int num = 0;
		string text = string.Empty;
		while (num < description.Length)
		{
			int num2 = description.IndexOf("{uicp|", num);
			if (num2 == -1)
			{
				text += description.Substring(num);
				break;
			}
			_ = description[num2];
			text += description.Substring(num, num2 - num);
			num = num2;
			num2 += description.Substring(num2).IndexOf('}');
			string link = description.Substring(num + 6, num2 - num - 6);
			EntityProperty link2 = StatTypeFromString(link);
			num = num2 + 1;
			link = GetCommonPropertyStringFromStatType(link2, m_Caster as UnitEntity);
			text += link;
		}
		return text;
	}

	private EntityProperty StatTypeFromString(string link)
	{
		return link switch
		{
			"BallisticSkill" => EntityProperty.BallisticSkill, 
			"WeaponSkill" => EntityProperty.WeaponSkill, 
			"Strength" => EntityProperty.Strength, 
			"Toughness" => EntityProperty.Toughness, 
			"Agility" => EntityProperty.Agility, 
			"Intelligence" => EntityProperty.Intelligence, 
			"Willpower" => EntityProperty.Willpower, 
			"Perception" => EntityProperty.Perception, 
			"Fellowship" => EntityProperty.Fellowship, 
			"BallisticSkillBonus" => EntityProperty.BallisticSkillBonus, 
			"WeaponSkillBonus" => EntityProperty.WeaponSkillBonus, 
			"StrengthBonus" => EntityProperty.StrengthBonus, 
			"ToughnessBonus" => EntityProperty.ToughnessBonus, 
			"AgilityBonus" => EntityProperty.AgilityBonus, 
			"IntelligenceBonus" => EntityProperty.IntelligenceBonus, 
			"WillpowerBonus" => EntityProperty.WillpowerBonus, 
			"PerceptionBonus" => EntityProperty.PerceptionBonus, 
			"FellowshipBonus" => EntityProperty.FellowshipBonus, 
			"Resolve" => EntityProperty.Resolve, 
			_ => EntityProperty.None, 
		};
	}

	private string GetCommonPropertyStringFromStatType(EntityProperty link, UnitEntity caster)
	{
		if (caster != null)
		{
			return link switch
			{
				EntityProperty.BallisticSkill => "{g|Encyclopedia:WarhammerBallisticSkill}" + caster.Stats.GetStat(StatType.WarhammerBallisticSkill)?.ToString() + "{/g}", 
				EntityProperty.WeaponSkill => "{g|Encyclopedia:WarhammerWeaponSkill}" + caster.Stats.GetStat(StatType.WarhammerWeaponSkill)?.ToString() + "{/g}", 
				EntityProperty.Strength => "{g|Encyclopedia:WarhammerStrength}" + caster.Stats.GetStat(StatType.WarhammerStrength)?.ToString() + "{/g}", 
				EntityProperty.Toughness => "{g|Encyclopedia:WarhammerToughness}" + caster.Stats.GetStat(StatType.WarhammerToughness)?.ToString() + "{/g}", 
				EntityProperty.Agility => "{g|Encyclopedia:WarhammerAgility}" + caster.Stats.GetStat(StatType.WarhammerAgility)?.ToString() + "{/g}", 
				EntityProperty.Intelligence => "{g|Encyclopedia:WarhammerIntelligence}" + caster.Stats.GetStat(StatType.WarhammerIntelligence)?.ToString() + "{/g}", 
				EntityProperty.Willpower => "{g|Encyclopedia:WarhammerWillpower}" + caster.Stats.GetStat(StatType.WarhammerWillpower)?.ToString() + "{/g}", 
				EntityProperty.Perception => "{g|Encyclopedia:WarhammerPerception}" + caster.Stats.GetStat(StatType.WarhammerPerception)?.ToString() + "{/g}", 
				EntityProperty.Fellowship => "{g|Encyclopedia:WarhammerFellowship}" + caster.Stats.GetStat(StatType.WarhammerFellowship)?.ToString() + "{/g}", 
				EntityProperty.BallisticSkillBonus => "{g|Encyclopedia:WarhammerBallisticSkill}" + (caster.Stats.GetAttributeOptional(StatType.WarhammerBallisticSkill)?.Bonus ?? 0) + "{/g}", 
				EntityProperty.WeaponSkillBonus => "{g|Encyclopedia:WarhammerWeaponSkill}" + (caster.Stats.GetAttributeOptional(StatType.WarhammerWeaponSkill)?.Bonus ?? 0) + "{/g}", 
				EntityProperty.StrengthBonus => "{g|Encyclopedia:WarhammerStrength}" + (caster.Stats.GetAttributeOptional(StatType.WarhammerStrength)?.Bonus ?? 0) + "{/g}", 
				EntityProperty.ToughnessBonus => "{g|Encyclopedia:WarhammerToughness}" + (caster.Stats.GetAttributeOptional(StatType.WarhammerToughness)?.Bonus ?? 0) + "{/g}", 
				EntityProperty.AgilityBonus => "{g|Encyclopedia:WarhammerAgility}" + (caster.Stats.GetAttributeOptional(StatType.WarhammerAgility)?.Bonus ?? 0) + "{/g}", 
				EntityProperty.IntelligenceBonus => "{g|Encyclopedia:WarhammerIntelligence}" + (caster.Stats.GetAttributeOptional(StatType.WarhammerIntelligence)?.Bonus ?? 0) + "{/g}", 
				EntityProperty.WillpowerBonus => "{g|Encyclopedia:WarhammerWillpower}" + (caster.Stats.GetAttributeOptional(StatType.WarhammerWillpower)?.Bonus ?? 0) + "{/g}", 
				EntityProperty.PerceptionBonus => "{g|Encyclopedia:WarhammerPerception}" + (caster.Stats.GetAttributeOptional(StatType.WarhammerPerception)?.Bonus ?? 0) + "{/g}", 
				EntityProperty.FellowshipBonus => "{g|Encyclopedia:WarhammerFellowship}" + (caster.Stats.GetAttributeOptional(StatType.WarhammerFellowship)?.Bonus ?? 0) + "{/g}", 
				EntityProperty.Resolve => "<u>{g|Encyclopedia:Resolve}resolve" + caster.Stats.GetStat(StatType.Resolve)?.ToString() + "{/g}</u>", 
				_ => "UNKNOWN STAT", 
			};
		}
		return link switch
		{
			EntityProperty.BallisticSkill => "{g|Encyclopedia:WarhammerBallisticSkill}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerBallisticSkill) + "{/g}", 
			EntityProperty.WeaponSkill => "{g|Encyclopedia:WarhammerWeaponSkill}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerWeaponSkill) + "{/g}", 
			EntityProperty.Strength => "{g|Encyclopedia:WarhammerStrength}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerStrength) + "{/g}", 
			EntityProperty.Toughness => "{g|Encyclopedia:WarhammerToughness}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerToughness) + "{/g}", 
			EntityProperty.Agility => "{g|Encyclopedia:WarhammerAgility}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerAgility) + "{/g}", 
			EntityProperty.Intelligence => "{g|Encyclopedia:WarhammerIntelligence}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerIntelligence) + "{/g}", 
			EntityProperty.Willpower => "{g|Encyclopedia:WarhammerWillpower}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerWillpower) + "{/g}", 
			EntityProperty.Perception => "{g|Encyclopedia:WarhammerPerception}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerPerception) + "{/g}", 
			EntityProperty.Fellowship => "{g|Encyclopedia:WarhammerFellowship}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerFellowship) + "{/g}", 
			EntityProperty.BallisticSkillBonus => "{g|Encyclopedia:WarhammerBallisticSkill}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerBallisticSkill) + "{/g}", 
			EntityProperty.WeaponSkillBonus => "{g|Encyclopedia:WarhammerWeaponSkill}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerWeaponSkill) + "{/g}", 
			EntityProperty.StrengthBonus => "{g|Encyclopedia:WarhammerStrength}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerStrength) + "{/g}", 
			EntityProperty.ToughnessBonus => "{g|Encyclopedia:WarhammerToughness}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerToughness) + "{/g}", 
			EntityProperty.AgilityBonus => "{g|Encyclopedia:WarhammerAgility}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerAgility) + "{/g}", 
			EntityProperty.IntelligenceBonus => "{g|Encyclopedia:WarhammerIntelligence}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerIntelligence) + "{/g}", 
			EntityProperty.WillpowerBonus => "{g|Encyclopedia:WarhammerWillpower}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerWillpower) + "{/g}", 
			EntityProperty.PerceptionBonus => "{g|Encyclopedia:WarhammerPerception}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerPerception) + "{/g}", 
			EntityProperty.FellowshipBonus => "{g|Encyclopedia:WarhammerFellowship}" + LocalizedTexts.Instance.Stats.GetText(StatType.WarhammerFellowship) + "{/g}", 
			EntityProperty.Resolve => "<u>{g|Encyclopedia:Resolve}resolve" + LocalizedTexts.Instance.Stats.GetText(StatType.Resolve) + "{/g}</u>", 
			EntityProperty.PsyRating => "{g|Encyclopedia:PsyRating}psy rating" + LocalizedTexts.Instance.Stats.GetText(StatType.PsyRating) + "{/g}", 
			_ => "UNKNOWN STAT", 
		};
	}

	private void AddMovementActionVeil(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (type == TooltipTemplateType.Info)
		{
			AddEndTurnInfo(list);
			AddAttckAbilityGroupCooldown(list);
			AddVeilDegradation(list);
		}
		else
		{
			string leftLine = string.Empty;
			Sprite sprite = null;
			if (!string.IsNullOrEmpty(m_EndTurn))
			{
				sprite = UIConfig.Instance.UIIcons.TooltipIcons.MoveEndPoints;
				leftLine = UIStrings.Instance.Tooltips.SpendAllMovementPointsShort;
			}
			string middleLine = string.Empty;
			Sprite sprite2 = null;
			if (!string.IsNullOrEmpty(m_AttackAbilityGroupCooldown))
			{
				sprite2 = UIConfig.Instance.UIIcons.TooltipIcons.ActionEndPoints;
				middleLine = UIStrings.Instance.Tooltips.AttackAbilityGroupCooldownShort;
			}
			string rightLine = string.Empty;
			Sprite sprite3 = null;
			if (!string.IsNullOrEmpty(m_Veil))
			{
				rightLine = UIStrings.Instance.Tooltips.IncreaseVeilDegradationShort;
				sprite3 = UIConfig.Instance.UIIcons.TooltipIcons.Vail;
			}
			if (sprite != null || sprite2 != null || sprite3 != null)
			{
				TextFieldParams textFieldParams = new TextFieldParams
				{
					FontColor = UIConfig.Instance.TooltipColors.Default,
					FontStyles = TMPro.FontStyles.Strikethrough
				};
				list.Add(new TooltipBrickTripleText(leftLine, middleLine, rightLine, sprite, sprite2, sprite3, textFieldParams, textFieldParams, textFieldParams));
			}
		}
		if (list.Count > 0)
		{
			bricks.Add(new TooltipBrickSeparator(TooltipBrickElementType.Medium));
			bricks.AddRange(list);
		}
	}

	private void AddEndTurnInfo(List<ITooltipBrick> bricks)
	{
		if (!string.IsNullOrEmpty(m_EndTurn))
		{
			bricks.Add(new TooltipBrickIconValueStat(m_EndTurn, null, UIConfig.Instance.UIIcons.TooltipIcons.MoveEndPoints, TooltipIconValueStatType.NameTextNormal, isWhite: true, needChangeSize: true, 18, 18, needChangeColor: false, default(Color), default(Color), useSecondaryLabelColor: true));
		}
	}

	private void AddAttckAbilityGroupCooldown(List<ITooltipBrick> bricks)
	{
		if (!string.IsNullOrEmpty(m_AttackAbilityGroupCooldown))
		{
			if (bricks.Count > 0)
			{
				bricks.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
			}
			bricks.Add(new TooltipBrickIconValueStat(m_AttackAbilityGroupCooldown, null, UIConfig.Instance.UIIcons.TooltipIcons.ActionEndPoints, TooltipIconValueStatType.NameTextNormal, isWhite: true, needChangeSize: true, 18, 18, needChangeColor: false, default(Color), default(Color), useSecondaryLabelColor: true));
		}
	}

	private void AddVeilDegradation(List<ITooltipBrick> bricks)
	{
		if (!string.IsNullOrEmpty(m_Veil))
		{
			if (bricks.Count > 0)
			{
				bricks.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
			}
			bricks.Add(new TooltipBrickIconValueStat(m_Veil, null, UIConfig.Instance.UIIcons.TooltipIcons.Vail, TooltipIconValueStatType.NameTextNormal, isWhite: true, needChangeSize: true, 18, 18, needChangeColor: false, default(Color), default(Color), useSecondaryLabelColor: true));
		}
	}
}
