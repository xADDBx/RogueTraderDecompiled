using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickFeatureVM : TooltipBaseBrickVM
{
	public readonly BlueprintFeatureBase Feature;

	public string Name;

	public Sprite Icon;

	public Color32 IconColor;

	public string Acronym;

	public TooltipBaseTemplate Tooltip;

	public bool IsHeader;

	public bool AvailableBackground;

	public FeatureTypes Type;

	public string AdditionalField1;

	public string AdditionalField2;

	public MechanicEntity Caster;

	public TooltipBrickFeatureVM(BlueprintFeatureBase feature, bool isHeader, bool available, bool showIcon = true, TooltipBaseTemplate tooltip = null, FeatureTypes type = FeatureTypes.Common, MechanicEntity caster = null)
	{
		Feature = feature;
		Caster = caster;
		IsHeader = isHeader;
		Name = feature.Name;
		Type = type;
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

	public TooltipBrickFeatureVM(string name, Sprite icon, bool isHeader, bool available, bool showIcon = true, TooltipBaseTemplate tooltip = null, FeatureTypes type = FeatureTypes.Common, MechanicEntity caster = null)
	{
		Caster = caster;
		IsHeader = isHeader;
		Name = name;
		Type = type;
		if (showIcon)
		{
			Icon = icon;
			IconColor = Color.white;
		}
		AvailableBackground = !available;
	}

	public TooltipBrickFeatureVM(BlueprintAbility ability, bool isHeader, TooltipBaseTemplate tooltip = null, FeatureTypes type = FeatureTypes.Common, MechanicEntity caster = null)
	{
		IsHeader = isHeader;
		Name = ability.Name;
		Type = type;
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

	public TooltipBrickFeatureVM(ActivatableAbility activatableAbility, bool isHeader, TooltipBaseTemplate tooltip = null, FeatureTypes type = FeatureTypes.Common)
	{
		IsHeader = isHeader;
		Name = activatableAbility.Name;
		Type = type;
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

	public TooltipBrickFeatureVM(BlueprintActivatableAbility activatableAbility, bool isHeader, TooltipBaseTemplate tooltip = null, FeatureTypes type = FeatureTypes.Common)
	{
		IsHeader = isHeader;
		Name = activatableAbility.Name;
		Type = type;
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

	public TooltipBrickFeatureVM(IUIDataProvider dataProvider, bool isHeader, TooltipBaseTemplate tooltip = null, FeatureTypes type = FeatureTypes.Common)
	{
		IsHeader = isHeader;
		Name = dataProvider.Name;
		Type = type;
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

	public TooltipBrickFeatureVM(UIUtilityItem.UIAbilityData uiAbilityData, bool isHeader, TooltipBaseTemplate tooltip = null, FeatureTypes type = FeatureTypes.Common)
	{
		IsHeader = isHeader;
		Name = uiAbilityData.Name;
		Type = type;
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
			AdditionalField1 = ((!uiAbilityData.IsSpaceCombatAbility) ? uiAbilityData.CostAP : string.Empty);
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
}
