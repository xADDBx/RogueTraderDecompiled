using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;

[JsonObject]
public class FeatureUIData : IFeatureSelectionItem, IUIDataProvider
{
	[JsonProperty]
	public BlueprintFeature Feature { get; }

	[JsonProperty]
	public FeatureParam Param { get; }

	[JsonProperty]
	public string Name { get; set; }

	[JsonProperty]
	public string Description { get; set; }

	[JsonProperty]
	public Sprite Icon { get; set; }

	[JsonProperty]
	public string NameForAcronym { get; set; }

	[JsonConstructor]
	public FeatureUIData(BlueprintFeature feature, FeatureParam param, string name, string description, Sprite icon, string nameForAcronym)
	{
		Feature = feature;
		Param = param;
		Name = name;
		Description = description;
		Icon = icon;
		NameForAcronym = nameForAcronym;
	}

	public FeatureUIData(BlueprintFeature feature, FeatureParam param = null)
	{
		Feature = feature;
		Param = param;
		Description = string.Empty;
		try
		{
			if (param == null)
			{
				Name = feature.Name;
				Description = feature.Description;
				Icon = feature.Icon;
				if (Icon == null)
				{
					Sprite sprite = UIUtilityUnit.GetBlueprintUnitFactFromFact<BlueprintAbility>(feature)?.FirstOrDefault()?.Icon;
					Icon = (sprite ? sprite : UIUtilityUnit.GetBlueprintUnitFactFromFact<BlueprintFeature>(feature)?.FirstOrDefault()?.Icon);
				}
				string abilityAcronym = UIUtility.GetAbilityAcronym(feature.Name);
				NameForAcronym = ((!string.IsNullOrEmpty(abilityAcronym)) ? abilityAcronym : UIUtility.GetAbilityAcronym(feature.NameForAcronym));
				return;
			}
			Name = feature.Name + " (" + GetParamName(param.Value) + ")";
			Description = GetParamDescription(param.Value);
			if (string.IsNullOrEmpty(Description))
			{
				Description = feature.Description;
			}
			Icon = GetParamIcon(param.Value);
			if (Icon == null)
			{
				Icon = feature.Icon;
			}
			NameForAcronym = GetParamAcronym(param.Value);
		}
		catch (Exception value)
		{
			System.Console.WriteLine(value);
			Name = "Error";
			NameForAcronym = "E";
		}
	}

	public FeatureUIData(Feature feature)
		: this(feature.Blueprint, feature.Param)
	{
	}

	private static string GetParamName(FeatureParam param)
	{
		if (param.StatType.HasValue)
		{
			return LocalizedTexts.Instance.Stats.GetText(param.StatType.Value);
		}
		if (param.WeaponCategory.HasValue)
		{
			return LocalizedTexts.Instance.Stats.GetText(param.WeaponCategory.Value);
		}
		if ((bool)param.Blueprint)
		{
			return (param.Blueprint as IUIDataProvider)?.Name;
		}
		return "";
	}

	private static string GetParamDescription(FeatureParam param)
	{
		return (param.Blueprint as IUIDataProvider)?.Description ?? "";
	}

	private static Sprite GetParamIcon(FeatureParam param)
	{
		return (param.Blueprint as IUIDataProvider)?.Icon;
	}

	private static string GetParamAcronym(FeatureParam param)
	{
		return SimpleBlueprintExtendAsObject.Or(param.Blueprint, null)?.name ?? param.StatType?.ToString() ?? param.SpellSchool?.ToString() ?? param.WeaponCategory?.ToString() ?? "";
	}
}
