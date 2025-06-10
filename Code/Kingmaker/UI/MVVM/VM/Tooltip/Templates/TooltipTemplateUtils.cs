using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.UI;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public static class TooltipTemplateUtils
{
	public static string GetFullDescription(BlueprintMechanicEntityFact blueprintMechanicEntityFact)
	{
		string description = blueprintMechanicEntityFact.Description;
		List<string> additionalDescription = GetAdditionalDescription(blueprintMechanicEntityFact);
		return AggregateDescription(description, additionalDescription);
	}

	public static List<string> GetAdditionalDescription(BlueprintFact blueprintFact)
	{
		return (from desc in blueprintFact.GetComponents<AdditionalDescriptionComponent>()
			where !desc.Disabled
			select desc.AdditionalDescription.Text).ToList();
	}

	public static string AggregateDescription(string description, List<string> additionalDesc)
	{
		if (description == null)
		{
			if (additionalDesc.Count == 0)
			{
				return null;
			}
			description = string.Empty;
		}
		return additionalDesc.Aggregate(description, (string current, string addDesc) => current + ((current.Length > 0) ? "\n" : "") + addDesc);
	}

	public static string UpdateDescriptionWithUIProperties(string description, MechanicEntity caster, BlueprintAbility blueprintAbility)
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
			if (caster != null)
			{
				Ability ability = caster.Facts.Get<Ability>(blueprintAbility);
				if ((ability?.GetComponent<UIPropertiesComponent>())?.Properties.First((UIPropertySettings property) => property.Name == name) != null)
				{
					string text2 = ability.GetComponent<PropertyCalculatorComponent>()?.GetValue(new PropertyContext(caster, ability)).ToString();
					if (text2 != null)
					{
						name = "<link=\"" + EntityLink.GetTag(EntityLink.Type.UIProperty) + ":" + blueprintAbility.AssetGuid + ":" + name + "\">{" + text2 + "}</link>";
					}
				}
			}
			else
			{
				UIPropertySettings uIPropertySettings = blueprintAbility.GetComponent<UIPropertiesComponent>()?.Properties.First((UIPropertySettings property) => property.Name == name);
				if (uIPropertySettings != null)
				{
					name = uIPropertySettings.Description;
				}
			}
			text += name;
		}
		return text;
	}
}
