using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks.Utils;
using Kingmaker.Controllers.Enums;
using Kingmaker.DLC;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Stores;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.Models.Tooltip;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
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

	public readonly MechanicEntity Caster;

	private string m_Name = string.Empty;

	private Sprite m_Icon;

	private string m_Type = string.Empty;

	private string m_Cost = string.Empty;

	private readonly string m_Level = string.Empty;

	private string m_Veil = string.Empty;

	private List<(string Text, Sprite Icon)> m_Targets;

	private string m_Cooldown = string.Empty;

	private string m_EndTurn = string.Empty;

	private string m_AttackAbilityGroupCooldown = string.Empty;

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

	private bool m_IsScreenWindowTooltip;

	private bool IsWeaponAbility
	{
		get
		{
			BlueprintItem sourceItem = SourceItem;
			return sourceItem is BlueprintItemWeapon || sourceItem is BlueprintStarshipItem;
		}
	}

	private bool IsSpaceCombatAbility => Game.Instance.CurrentMode == GameModeType.SpaceCombat;

	protected virtual MechanicEntity PreviewEntity => Caster;

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

	public TooltipTemplateAbility(BlueprintAbility blueprintAbility, BlueprintItem sourceItem = null, MechanicEntity caster = null, bool isScreenWindowTooltip = false)
	{
		BlueprintAbility = blueprintAbility;
		SourceItem = sourceItem;
		Caster = caster;
		m_IsScreenWindowTooltip = isScreenWindowTooltip;
	}

	public TooltipTemplateAbility(AbilityData abilityData, bool isScreenWindowTooltip = false)
	{
		AbilityData = abilityData;
		m_DamageInfo = null;
		BlueprintAbility = abilityData.Blueprint;
		Caster = abilityData.Caster;
		SourceItem = abilityData.SourceItem?.Blueprint;
		m_Weapon = abilityData.SourceItem as ItemEntityWeapon;
		m_IsScreenWindowTooltip = isScreenWindowTooltip;
	}

	private void FillBlueprintAbilityData(BlueprintAbility blueprintAbility)
	{
		try
		{
			using (ContextData<DisableStatefulRandomContext>.Request())
			{
				BlueprintItemWeapon blueprintItemWeapon = SourceItem as BlueprintItemWeapon;
				m_Name = blueprintAbility.Name;
				m_Icon = blueprintAbility.Icon;
				m_Type = GetAbilityType(blueprintAbility);
				m_Targets = GetTargets(blueprintAbility, blueprintItemWeapon).ToList();
				m_Cooldown = blueprintAbility.CooldownRounds.ToString();
				m_IsOnTimeInBattleAbility = CheckOneTimeInBattleAbility(blueprintAbility);
				m_EndTurn = GetEndTurn(blueprintAbility);
				m_AttackAbilityGroupCooldown = GetAttackAbilityGroupCooldown(blueprintAbility);
				m_ShortDescriptionText = blueprintAbility.GetShortenedDescription();
				m_LongDescriptionText = blueprintAbility.Description;
				m_SpellDescriptor = UIUtilityTexts.GetSpellDescriptorsText(blueprintAbility);
				m_ActionTime = UIUtilityTexts.GetAbilityActionText(blueprintAbility);
				m_UIAbilityData = UIUtilityItem.GetUIAbilityData(blueprintAbility, blueprintItemWeapon, Caster);
				m_IsReload = UIUtilityItem.IsReload(blueprintAbility);
			}
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
			using (ContextData<DisableStatefulRandomContext>.Request())
			{
				using (ContextData<UnitHelper.PreviewUnit>.Request())
				{
					using (ContextData<UnitHelper.DoNotCreateItems>.Request())
					{
						using (GameLogContext.Scope)
						{
							GameLogContext.UnitEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)Caster;
							m_Name = abilityData.Name;
							m_Icon = abilityData.Icon;
							m_Type = GetAbilityType(abilityData.Blueprint);
							m_Cost = GetCost(abilityData);
							m_Veil = GetVeil(abilityData);
							m_EndTurn = GetEndTurn(abilityData.Blueprint);
							m_AttackAbilityGroupCooldown = GetAttackAbilityGroupCooldown(abilityData.Blueprint);
							m_Targets = GetTargets(abilityData).ToList();
							m_Cooldown = abilityData.Blueprint.CooldownRounds.ToString();
							m_IsOnTimeInBattleAbility = CheckOneTimeInBattleAbility(abilityData.Blueprint);
							m_ShortDescriptionText = abilityData.ShortenedDescription;
							m_LongDescriptionText = abilityData.Description;
							m_SpellDescriptor = UIUtilityTexts.GetSpellDescriptorsText(abilityData.Blueprint);
							m_ActionTime = UIUtilityTexts.GetAbilityActionText(abilityData);
							m_UIAbilityData = UIUtilityItem.GetUIAbilityData(abilityData.Blueprint, abilityData.Weapon);
							m_IsReload = UIUtilityItem.IsReload(abilityData);
						}
					}
				}
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
		AddCantUseInfo(list);
		AddDamageInfo(list);
		AddTargets(list);
		AddCooldown(list);
		AddHitChances(list);
		AddAttackOfOpportunity(list);
		AddDescription(list, type);
		AddMovementActionVeil(list, type);
		TryAddRetargetableIconBrick(list, type);
		TryAddRedirectDrivenBrick(list);
		return list;
	}

	private void TryAddRetargetableIconBrick(List<ITooltipBrick> result, TooltipTemplateType type)
	{
		if (StoreManager.GetPurchasableDLCs().OfType<BlueprintDlc>().FirstOrDefault((BlueprintDlc dlc) => dlc.DlcType == DlcTypeEnum.AdditionalContentDlc && dlc.IsPurchased && dlc.IsEnabled && dlc.name == "DLC2LexImperialis") == null || BlueprintAbility == null)
		{
			return;
		}
		AbilityRedirect component = BlueprintAbility.GetComponent<AbilityRedirect>();
		if (component == null || (AbilityData != null && !AbilityData.CanRedirect))
		{
			return;
		}
		if (type == TooltipTemplateType.Tooltip)
		{
			TextFieldParams textFieldParams = new TextFieldParams
			{
				FontColor = UIConfig.Instance.TooltipColors.Default,
				FontStyles = TMPro.FontStyles.Strikethrough
			};
			if (component.CasterRestrictions.Property.Empty)
			{
				textFieldParams.FontStyles = TMPro.FontStyles.Normal;
			}
			result.Add(new TooltipBrickTripleText(UIStrings.Instance.Tooltips.RetargetableAbilityTooltip, string.Empty, string.Empty, UIConfig.Instance.UIIcons.TooltipIcons.PetPawIcon, null, null, textFieldParams));
		}
		else
		{
			TextFieldParams textFieldParams2 = new TextFieldParams
			{
				FontColor = UIConfig.Instance.TooltipColors.Default,
				FontStyles = TMPro.FontStyles.Normal
			};
			LocalizedString retargetableAbilityTooltip = UIStrings.Instance.Tooltips.RetargetableAbilityTooltip;
			LocalizedString localizedString = (component.CasterRestrictions.Property.Empty ? UIStrings.Instance.Tooltips.RetargetableAbilityTooltipCanBeUsed : UIStrings.Instance.Tooltips.RetargetableAbilityTooltipCanBeUsedUltimate);
			result.Add(new TooltipBrickTripleText(retargetableAbilityTooltip, "-", localizedString, UIConfig.Instance.UIIcons.TooltipIcons.PetPawIcon, null, null, textFieldParams2, null, textFieldParams2));
		}
	}

	private void TryAddRedirectDrivenBrick(List<ITooltipBrick> result)
	{
		if (BlueprintAbility != null && Caster != null)
		{
			PartAbilityRedirect optional = Caster.GetOptional<PartAbilityRedirect>();
			if (!(BlueprintAbility.name != "RavenPet_Cycle_Ability") && optional?.LastUsedAbility.Fact != null)
			{
				result.Add(new TooltipBrickLastUsedAbilityPaper(optional?.LastUsedAbility.Fact.Data.Name, optional?.LastUsedAbility.Fact.Data.Icon));
			}
		}
	}

	private void AddCantUseInfo(List<ITooltipBrick> result)
	{
		if (BlueprintAbility == null || Caster == null)
		{
			return;
		}
		AbilityCasterHasNoFacts component = BlueprintAbility.GetComponent<AbilityCasterHasNoFacts>();
		AbilityCasterHasFacts component2 = BlueprintAbility.GetComponent<AbilityCasterHasFacts>();
		if (component != null && component.Facts.Length != 0)
		{
			foreach (BlueprintUnitFact fact in component.Facts)
			{
				if (Caster.Facts.Contains(fact))
				{
					result.Add(new TooltipBrickCantUsePaper(UIStrings.Instance.Tooltips.CantUseRemove, fact.LocalizedName, fact.Icon));
				}
			}
		}
		if (component2 == null || component2.Facts.Length == 0)
		{
			return;
		}
		foreach (BlueprintUnitFact fact2 in component2.Facts)
		{
			if (!Caster.Facts.Contains(fact2))
			{
				result.Add(new TooltipBrickCantUsePaper(UIStrings.Instance.Tooltips.CantUseNeed, fact2.LocalizedName, fact2.Icon));
			}
		}
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
			BlueprintItemWeapon obj = SourceItem as BlueprintItemWeapon;
			string text = ((obj != null && obj.IsMelee) ? UIStrings.Instance.Tooltips.MeleeStrikesCount : UIStrings.Instance.Tooltips.ShotsCount);
			text = text.Replace("{0}", "");
			tertiaryValues = new TooltipBrickIconPattern.TextFieldValues
			{
				Text = text,
				Value = m_UIAbilityData.BurstAttacksCount.ToString()
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
					Text = string.Format(UIStrings.Instance.Tooltips.ShotsCount, $"{currentAmmo}/{warhammerMaxAmmo}")
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
		string text = ((component == null) ? string.Empty : ((string)(component.clearMPInsteadOfEndingTurn ? UIStrings.Instance.Tooltips.SpendAllMovementPoints : UIStrings.Instance.Tooltips.EndsTurn)));
		CheckBuffForMPSpendTooltip component2 = blueprintAbility.GetComponent<CheckBuffForMPSpendTooltip>();
		BaseUnitEntity currentSelectedUnit = UIUtility.GetCurrentSelectedUnit();
		return (component2 == null || currentSelectedUnit == null) ? text : (component2.CheckContainsBuff(currentSelectedUnit) ? ((string)UIStrings.Instance.Tooltips.SpendAllMovementPoints) : string.Empty);
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

	private static IEnumerable<(string, Sprite)> GetTargets(BlueprintAbility blueprintAbility, BlueprintItemWeapon weaponBlueprint)
	{
		if (!blueprintAbility.TryGetComponent<IAbilityMultiTarget>(out var component))
		{
			return new(string, Sprite)[1] { (UIUtilityTexts.GetAbilityTarget(blueprintAbility, weaponBlueprint), UIUtilityTexts.GetTargetImage(blueprintAbility)) };
		}
		return component.GetAllTargetsForTooltip(new AbilityData(blueprintAbility, UIUtility.GetCurrentSelectedUnit())).Select(AbilityToTargetInfo);
	}

	private static IEnumerable<(string, Sprite)> GetTargets(AbilityData abilityData)
	{
		return GetTargetsFromAbility(abilityData).Select(AbilityToTargetInfo);
	}

	private static (string, Sprite) AbilityToTargetInfo(AbilityData abilityData)
	{
		return (UIUtilityTexts.GetAbilityTarget(abilityData), UIUtilityTexts.GetTargetImage(abilityData.Blueprint));
	}

	private static IEnumerable<AbilityData> GetTargetsFromAbility(AbilityData abilityData)
	{
		if (!abilityData.Blueprint.TryGetComponent<IAbilityMultiTarget>(out var component))
		{
			return new AbilityData[1] { abilityData };
		}
		return component.GetAllTargetsForTooltip(abilityData);
	}

	private void AddTargets(List<ITooltipBrick> bricks)
	{
		if (m_IsReload || m_Targets == null)
		{
			return;
		}
		foreach (var (text, sprite) in m_Targets)
		{
			if (!string.IsNullOrEmpty(text) && !(sprite == null))
			{
				TooltipBrickIconPattern.TextFieldValues titleValues = new TooltipBrickIconPattern.TextFieldValues
				{
					Text = string.Empty
				};
				TooltipBrickIconPattern.TextFieldValues secondaryValues = new TooltipBrickIconPattern.TextFieldValues
				{
					Text = text
				};
				bricks.Add(new TooltipBrickIconPattern(sprite, m_UIAbilityData.PatternData, titleValues, secondaryValues, null, null, IconPatternMode.IconMode));
			}
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
			TooltipBrickIconPattern.TextFieldValues textFieldValues = new TooltipBrickIconPattern.TextFieldValues();
			if (m_IsOnTimeInBattleAbility)
			{
				textFieldValues.Text = UIUtilityTexts.WrapWithWeight(UIStrings.Instance.TurnBasedTexts.CanUseOneTimeInCombat, TextFontWeight.SemiBold);
			}
			else
			{
				textFieldValues.Text = UIStrings.Instance.TurnBasedTexts.Rounds;
				textFieldValues.Value = m_Cooldown;
			}
			bricks.Add(new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.Cooldown, null, titleValues, textFieldValues, null, null, IconPatternMode.IconMode));
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
			bricks.Add(new TooltipBrickIconStatValue(UIStrings.Instance.Tooltips.HitChances, UIConfig.Instance.PercentHelper.AddPercentTo(m_UIAbilityData.HitChance)));
		}
	}

	private void AddRangeHitChances(List<ITooltipBrick> bricks)
	{
		TextFieldParams textParams = new TextFieldParams
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
			Value = UIConfig.Instance.PercentHelper.AddPercentTo(m_UIAbilityData.HitChance),
			TextParams = textParams
		};
		TooltipBrickIconPattern.TextFieldValues tertiaryValues = new TooltipBrickIconPattern.TextFieldValues
		{
			Text = UIStrings.Instance.Tooltips.HitChancesMaxDistance,
			Value = UIConfig.Instance.PercentHelper.AddPercentTo(m_UIAbilityData.HitChance / 2),
			TextParams = textParams
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
		bricks.Add(new TooltipBrickIconStatValue(tooltips.ScatterMainLineClose, UIConfig.Instance.PercentHelper.AddPercentTo(scatterHitChanceData.MainLineClose)));
		bricks.Add(new TooltipBrickIconStatValue(tooltips.ScatterClose, UIConfig.Instance.PercentHelper.AddPercentTo(scatterHitChanceData.ScatterClose)));
		bricks.Add(new TooltipBricksGroupEnd());
		bricks.Add(new TooltipBricksGroupStart());
		bricks.Add(new TooltipBrickText(UIStrings.Instance.Tooltips.HitChancesMaxDistance, TooltipTextType.Bold));
		bricks.Add(new TooltipBrickIconStatValue(tooltips.ScatterMainLine, UIConfig.Instance.PercentHelper.AddPercentTo(scatterHitChanceData.MainLine)));
		bricks.Add(new TooltipBrickIconStatValue(tooltips.ScatterNear, UIConfig.Instance.PercentHelper.AddPercentTo(scatterHitChanceData.ScatterNear)));
		bricks.Add(new TooltipBrickIconStatValue(tooltips.ScatterFar, UIConfig.Instance.PercentHelper.AddPercentTo(scatterHitChanceData.ScatterFar)));
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
		if (m_IsReload)
		{
			return;
		}
		string baseDamageText = m_UIAbilityData.BaseDamageText;
		string text = UIConfig.Instance.PercentHelper.AddPercentTo(m_UIAbilityData.Penetration);
		if (!string.IsNullOrEmpty(baseDamageText) && !(baseDamageText == "0-0") && !(text == "0%"))
		{
			Sprite damage = UIConfig.Instance.UIIcons.Damage;
			Sprite penetration = UIConfig.Instance.UIIcons.Penetration;
			string text2 = UIStrings.Instance.TooltipsElementLabels.GetLabel(TooltipElement.Damage);
			string damageType = GetDamageType();
			if (!string.IsNullOrWhiteSpace(damageType))
			{
				text2 = damageType + " " + text2.ToLower();
			}
			string label = UIStrings.Instance.TooltipsElementLabels.GetLabel(TooltipElement.Penetration);
			bricks.Add(new TooltipBrickTwoColumnsStat(text2, label, baseDamageText, text, damage, penetration));
		}
	}

	private string GetDamageType()
	{
		WarhammerOverrideAbilityWeapon warhammerOverrideAbilityWeapon = BlueprintAbility?.GetComponent<WarhammerOverrideAbilityWeapon>();
		if (warhammerOverrideAbilityWeapon?.Weapon == null)
		{
			return string.Empty;
		}
		return UIUtilityTexts.GetTextByKey(warhammerOverrideAbilityWeapon.Weapon.DamageType.Type);
	}

	private void AddAttackOfOpportunity(List<ITooltipBrick> bricks)
	{
		if (!(AbilityData == null) && ((BaseUnitEntity)AbilityData.Caster).CalculateAttackOfOpportunity(AbilityData).Count() != 0)
		{
			bricks.Add(new TooltipBrickAttackOfOpportunityPaper());
		}
	}

	protected virtual void AddDescription(List<ITooltipBrick> bricks, TooltipTemplateType type)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			using (ContextData<UnitHelper.PreviewUnit>.Request())
			{
				using (ContextData<UnitHelper.DoNotCreateItems>.Request())
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
						description = UIUtilityTexts.UpdateDescriptionWithUIProperties(description, PreviewEntity);
						description = UpdateDescriptionWithUICommonProperties(description);
						bricks.Add(new TooltipBrickText(description, TooltipTextType.Paragraph, isHeader: false, TooltipTextAlignment.Midl, needChangeSize: false, 18, PreviewEntity));
					}
				}
			}
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
			link = GetCommonPropertyStringFromStatType(link2, Caster as UnitEntity);
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
			bricks.Add(new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.MoveEndPoints, null, m_EndTurn, null, null, null, IconPatternMode.IconMode));
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
			bricks.Add(new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.ActionEndPoints, null, m_AttackAbilityGroupCooldown, null, null, null, IconPatternMode.IconMode));
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
			bricks.Add(new TooltipBrickIconPattern(UIConfig.Instance.UIIcons.TooltipIcons.Vail, null, m_Veil, null, null, null, IconPatternMode.IconMode));
		}
	}
}
