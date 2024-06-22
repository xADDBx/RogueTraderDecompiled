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
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.UI;
using Kingmaker.Utility.UnitDescription;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateShipAbility : TooltipBaseTemplate
{
	public readonly BlueprintAbility BlueprintAbility;

	private readonly MechanicEntity m_Caster;

	private readonly string m_Target = string.Empty;

	private readonly Sprite m_TargetIcon;

	private readonly string m_Duration = string.Empty;

	private readonly string m_Cooldown = string.Empty;

	private readonly string m_EndTurn = string.Empty;

	private readonly string m_AttackAbilityGroupCooldown = string.Empty;

	private readonly string m_ShortDescriptionText = string.Empty;

	private readonly string m_LongDescriptionText = string.Empty;

	private readonly UnitDescription.UIDamageInfo[] m_DamageInfo;

	private readonly string m_AutoCastHint = string.Empty;

	private readonly UIUtilityItem.UIAbilityData m_UIAbilityData;

	private readonly bool m_IsReload;

	private ItemEntityWeapon m_Weapon;

	private bool IsSpaceCombatAbility => Game.Instance.CurrentMode == GameModeType.SpaceCombat;

	public TooltipTemplateShipAbility(BlueprintAbility blueprintAbility, BlueprintItem sourceItem = null)
	{
		try
		{
			BlueprintAbility = blueprintAbility;
			m_Caster = null;
			BlueprintItemWeapon blueprintItem = sourceItem as BlueprintItemWeapon;
			m_Target = UIUtilityTexts.GetAbilityTarget(blueprintAbility, blueprintItem);
			m_TargetIcon = UIUtilityTexts.GetTargetImage(blueprintAbility);
			m_Duration = blueprintAbility.LocalizedDuration;
			m_Cooldown = blueprintAbility.CooldownRounds.ToString();
			m_EndTurn = GetEndTurn(blueprintAbility);
			m_AttackAbilityGroupCooldown = GetAttackAbilityGroupCooldown(blueprintAbility);
			m_ShortDescriptionText = blueprintAbility.GetShortenedDescription();
			m_LongDescriptionText = blueprintAbility.Description;
			m_UIAbilityData = UIUtilityItem.GetUIAbilityData(blueprintAbility, blueprintItem);
			m_IsReload = UIUtilityItem.IsReload(blueprintAbility);
		}
		catch (Exception arg)
		{
			Debug.LogError($"Can't create TooltipTemplate for: {blueprintAbility?.name}: {arg}");
		}
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
		AddUIProperties(list);
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
			textFieldValues.Text = ((!IsSpaceCombatAbility) ? UIStrings.Instance.Tooltips.CostAP.Text : string.Empty);
			textFieldValues.Value = ((!IsSpaceCombatAbility) ? m_UIAbilityData.CostAP : string.Empty);
			textFieldValues.TextParams.FontColor = Color.black;
			break;
		}
		return new TooltipBrickIconPattern(m_UIAbilityData.Icon, null, titleValues, textFieldValues, tertiaryValues);
	}

	private string GetCost(int cost, BlueprintAbility blueprintAbility)
	{
		if (blueprintAbility.AbilityParamsSource != WarhammerAbilityParamsSource.PsychicPower)
		{
			return string.Format(UIStrings.Instance.Tooltips.CostAP, cost);
		}
		return string.Format(UIStrings.Instance.Tooltips.PsychicPowerCostAP, cost, blueprintAbility.GetVeilThicknessPointsToAdd());
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
		if (!m_IsReload && !string.IsNullOrEmpty(m_Target) && !(m_TargetIcon == null))
		{
			TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = string.Empty
			};
			TooltipBrickIconPattern.TextFieldValues secondaryValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = m_Target
			};
			bricks.Add(new TooltipBrickIconPattern(m_TargetIcon, m_UIAbilityData.PatternData, titleValues, secondaryValues, null, null, IconPatternMode.IconMode));
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
			bricks.Add(new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.Duration, null, titleValues, secondaryValues, null, null, IconPatternMode.IconMode));
		}
	}

	private void AddCooldown(List<ITooltipBrick> bricks)
	{
		if (!string.IsNullOrEmpty(m_Cooldown) && !(m_Cooldown == "0"))
		{
			TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = UIStrings.Instance.TooltipsElementLabels.GetLabel(TooltipElement.Cooldown)
			};
			TooltipBrickIconPattern.TextFieldValues secondaryValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = UIUtilityTexts.WrapWithWeight(string.Concat(UIStrings.Instance.TurnBasedTexts.Rounds, ": ", m_Cooldown), TextFontWeight.SemiBold)
			};
			bricks.Add(new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.Cooldown, null, titleValues, secondaryValues, null, null, IconPatternMode.IconMode));
		}
	}

	private void AddHitChances(List<ITooltipBrick> bricks)
	{
		if (m_IsReload || IsSpaceCombatAbility)
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
		bricks.Add(new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.HitChances, null, titleValues, secondaryValues, tertiaryValues, null, IconPatternMode.IconMode));
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

	private void AddDamageInfo(List<ITooltipBrick> bricks)
	{
		if (!m_IsReload)
		{
			string damageText = m_UIAbilityData.DamageText;
			string text = ((m_UIAbilityData.Penetration > 0) ? $"{m_UIAbilityData.Penetration}" : string.Empty);
			if (!string.IsNullOrEmpty(damageText) && !string.IsNullOrEmpty(text))
			{
				Sprite damage = UIConfig.Instance.UIIcons.Damage;
				Sprite penetration = UIConfig.Instance.UIIcons.Penetration;
				string label = UIStrings.Instance.TooltipsElementLabels.GetLabel(TooltipElement.Damage);
				string label2 = UIStrings.Instance.TooltipsElementLabels.GetLabel(TooltipElement.Penetration);
				bricks.Add(new TooltipBrickTwoColumnsStat(label, label2, damageText, text, damage, penetration));
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
			description = UpdateDescriptionWithUIProperties(description);
			bricks.Add(new TooltipBrickText("\n" + description + "\n\n", TooltipTextType.Paragraph));
		}
	}

	private string UpdateDescriptionWithUIProperties(string description)
	{
		int num = 0;
		string text = "";
		while (num < description.Length)
		{
			int num2 = description.IndexOf("{uip|", num);
			if (num2 == -1)
			{
				text += description.Substring(num);
				break;
			}
			_ = description[num2];
			text += description.Substring(num, num2 - num);
			num = num2;
			num2 = description.IndexOf("}");
			string name = description.Substring(num + 5, num2 - num - 5);
			num = num2 + 1;
			if (m_Caster != null)
			{
				Ability ability = m_Caster.Facts.Get<Ability>(BlueprintAbility);
				if ((ability?.GetComponent<UIPropertiesComponent>())?.Properties.First((UIPropertySettings property) => property.Name == name) != null)
				{
					string text2 = ability.GetComponent<PropertyCalculatorComponent>()?.GetValue(new PropertyContext(m_Caster, ability)).ToString();
					if (text2 != null)
					{
						name = "<link=\"uip:" + BlueprintAbility.AssetGuid + ":" + name + "\">{" + text2 + "}</link>";
					}
				}
			}
			else
			{
				UIPropertySettings uIPropertySettings = BlueprintAbility.GetComponent<UIPropertiesComponent>()?.Properties.First((UIPropertySettings property) => property.Name == name);
				if (uIPropertySettings != null)
				{
					name = uIPropertySettings.Description;
				}
			}
			text += name;
		}
		return text;
	}

	private void AddMovementActionVeil(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (type == TooltipTemplateType.Info)
		{
			AddEndTurnInfo(list);
			AddAttckAbilityGroupCooldown(list);
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
			if (sprite != null || sprite2 != null)
			{
				TextFieldParams textFieldParams = new TextFieldParams
				{
					FontColor = UIConfig.Instance.TooltipColors.Default,
					FontStyles = TMPro.FontStyles.Strikethrough
				};
				list.Add(new TooltipBrickTripleText(leftLine, middleLine, string.Empty, sprite, sprite2, null, textFieldParams, textFieldParams, textFieldParams));
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
}
