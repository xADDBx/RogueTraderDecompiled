using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Blueprints;

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
}
