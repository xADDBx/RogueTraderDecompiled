using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickFeatureVM : TooltipBaseBrickVM
{
	public readonly BlueprintFeature Feature;

	public readonly BlueprintAbility Ability;

	public string Name;

	public Sprite Icon;

	public Color32 IconColor;

	public string Acronym;

	public TooltipBaseTemplate Tooltip;

	public readonly bool IsHeader;

	public bool AvailableBackground;

	public readonly string AdditionalField1;

	public readonly string AdditionalField2;

	public readonly MechanicEntity Caster;

	public readonly TalentIconInfo TalentIconsInfo;

	public readonly bool IsHidden;

	public readonly bool HasTalentsGroups;

	public bool HasFeature => Feature != null;

	public bool HasAbility => Ability != null;

	public TooltipBrickFeatureVM(BlueprintFeature feature, bool isHeader, bool available, bool showIcon = true, TooltipBaseTemplate tooltip = null, MechanicEntity caster = null, bool forceSetName = false, bool isHidden = false)
	{
		Feature = feature;
		Caster = caster;
		IsHeader = isHeader;
		IsHidden = isHidden;
		string name = feature.Name;
		TalentIconsInfo = feature.TalentIconInfo;
		HasTalentsGroups = UIUtilityItem.ShouldShowTalentIcons(feature);
		if (forceSetName && !string.IsNullOrWhiteSpace(feature.ForceSetNameForItemTooltip))
		{
			name = feature.ForceSetNameForItemTooltip;
		}
		Name = name;
		if (showIcon)
		{
			if (feature.Icon != null)
			{
				Icon = feature.Icon;
				IconColor = Color.white;
			}
			else
			{
				Icon = UIUtility.GetIconByText(feature.Name);
				IconColor = UIUtility.GetColorByText(feature.Name);
				Acronym = UIUtility.GetAbilityAcronym(feature);
			}
		}
		AvailableBackground = !available;
		if (!isHeader)
		{
			Tooltip = tooltip ?? new TooltipTemplateFeature(feature, withVariants: false, Caster);
		}
	}

	public TooltipBrickFeatureVM(string name, Sprite icon, bool isHeader, bool available, bool showIcon = true, TooltipBaseTemplate tooltip = null, MechanicEntity caster = null)
	{
		Caster = caster;
		IsHeader = isHeader;
		Name = name;
		if (showIcon)
		{
			Icon = icon;
			IconColor = Color.white;
		}
		AvailableBackground = !available;
		Tooltip = tooltip;
	}

	public TooltipBrickFeatureVM(BlueprintAbility ability, bool isHeader, TooltipBaseTemplate tooltip = null, MechanicEntity caster = null, bool isHidden = false)
	{
		Ability = ability;
		Caster = caster;
		IsHeader = isHeader;
		IsHidden = isHidden;
		Name = ability.Name;
		if (ability.Icon != null)
		{
			Icon = ability.Icon;
			IconColor = Color.white;
		}
		else
		{
			Icon = UIUtility.GetIconByText(ability.Name);
			IconColor = UIUtility.GetColorByText(ability.Name);
			Acronym = UIUtility.GetAbilityAcronym(ability.Name);
		}
		Tooltip = tooltip ?? new TooltipTemplateAbility(ability, null, caster);
	}

	public TooltipBrickFeatureVM(ActivatableAbility activatableAbility, bool isHeader, TooltipBaseTemplate tooltip = null)
	{
		IsHeader = isHeader;
		Name = activatableAbility.Name;
		if (activatableAbility.Icon != null)
		{
			Icon = activatableAbility.Icon;
			IconColor = Color.white;
		}
		else
		{
			Icon = UIUtility.GetIconByText(activatableAbility.Name);
			IconColor = UIUtility.GetColorByText(activatableAbility.Name);
			Acronym = UIUtility.GetAbilityAcronym(activatableAbility.Name);
		}
		Tooltip = tooltip ?? new TooltipTemplateActivatableAbility(activatableAbility);
	}

	public TooltipBrickFeatureVM(BlueprintActivatableAbility activatableAbility, bool isHeader, TooltipBaseTemplate tooltip = null)
	{
		IsHeader = isHeader;
		Name = activatableAbility.Name;
		if (activatableAbility.Icon != null)
		{
			Icon = activatableAbility.Icon;
			IconColor = Color.white;
		}
		else
		{
			Icon = UIUtility.GetIconByText(activatableAbility.Name);
			IconColor = UIUtility.GetColorByText(activatableAbility.Name);
			Acronym = UIUtility.GetAbilityAcronym(activatableAbility.Name);
		}
		Tooltip = tooltip ?? new TooltipTemplateActivatableAbility(activatableAbility);
	}

	public TooltipBrickFeatureVM(IUIDataProvider dataProvider, bool isHeader, TooltipBaseTemplate tooltip = null)
	{
		IsHeader = isHeader;
		Name = dataProvider.Name;
		if (dataProvider.Icon != null)
		{
			Icon = dataProvider.Icon;
			IconColor = Color.white;
		}
		else
		{
			Icon = UIUtility.GetIconByText(dataProvider.NameForAcronym);
			IconColor = UIUtility.GetColorByText(dataProvider.NameForAcronym);
			Acronym = UIUtility.GetAbilityAcronym(dataProvider.Name);
		}
		if (tooltip != null)
		{
			Tooltip = tooltip;
		}
		else if (dataProvider is BlueprintAbility blueprintAbility)
		{
			Tooltip = new TooltipTemplateAbility(blueprintAbility);
		}
		else
		{
			Tooltip = new TooltipTemplateDataProvider(dataProvider);
		}
	}

	public TooltipBrickFeatureVM(UIUtilityItem.UIAbilityData uiAbilityData, bool isHeader, TooltipBaseTemplate tooltip = null)
	{
		IsHeader = isHeader;
		Name = uiAbilityData.Name;
		if (uiAbilityData.Icon != null)
		{
			Icon = uiAbilityData.Icon;
			IconColor = Color.white;
		}
		else
		{
			Icon = UIUtility.GetIconByText(uiAbilityData.Name);
			IconColor = UIUtility.GetColorByText(uiAbilityData.Name);
			Acronym = UIUtility.GetAbilityAcronym(uiAbilityData.Name);
		}
		Tooltip = tooltip ?? new TooltipTemplateAbility(uiAbilityData.BlueprintAbility);
		switch (uiAbilityData.MomentumAbilityType)
		{
		case MomentumAbilityType.HeroicAct:
			AdditionalField1 = UIStrings.Instance.Tooltips.HeroicActAbility;
			break;
		case MomentumAbilityType.DesperateMeasure:
			AdditionalField1 = UIStrings.Instance.Tooltips.DesperateMeasureAbility;
			break;
		default:
			AdditionalField1 = ((!uiAbilityData.IsSpaceCombatAbility) ? (UIStrings.Instance.Tooltips.CostAP.Text + " " + uiAbilityData.CostAP) : string.Empty);
			break;
		}
		if (uiAbilityData.BurstAttacksCount > 1)
		{
			AdditionalField2 = string.Format(UIStrings.Instance.Tooltips.ShotsCount, uiAbilityData.BurstAttacksCount.ToString());
		}
		if (uiAbilityData.IsReload)
		{
			if (uiAbilityData.Weapon != null)
			{
				int currentAmmo = uiAbilityData.Weapon.CurrentAmmo;
				int warhammerMaxAmmo = uiAbilityData.Weapon.Blueprint.WarhammerMaxAmmo;
				AdditionalField2 = string.Format(UIStrings.Instance.Tooltips.ShotsCount, $"{currentAmmo}/{warhammerMaxAmmo}");
			}
			else
			{
				UberDebug.LogError("Error: Weapon is null");
			}
		}
	}

	protected TooltipBrickFeatureVM()
	{
	}

	public UnitPartCultAmbush.VisibilityStatuses UpdateCultAmbushVisibility()
	{
		if (HasAbility)
		{
			return Ability.CultAmbushVisibility((BaseUnitEntity)Caster, isFirstShow: true);
		}
		if (HasFeature)
		{
			return Feature.CultAmbushVisibility((BaseUnitEntity)Caster, isFirstShow: true);
		}
		return UnitPartCultAmbush.VisibilityStatuses.Visible;
	}
}
