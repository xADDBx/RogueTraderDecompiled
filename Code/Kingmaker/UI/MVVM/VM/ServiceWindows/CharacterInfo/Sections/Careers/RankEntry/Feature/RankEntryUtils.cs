using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Levelup.Selections.Prerequisites;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;

public static class RankEntryUtils
{
	public static BlueprintAbility GetAbilityFromFeature(BlueprintFeature feature)
	{
		AddFacts component = feature.GetComponent<AddFacts>();
		return ((component != null) ? component.Facts.FirstOrDefault() : null) as BlueprintAbility;
	}

	public static bool IsCommonSelectionItem(FeatureSelectionItem item)
	{
		if (!(item.SourceBlueprint is BlueprintCareerPath))
		{
			if (item.SourceBlueprint is BlueprintFeature blueprintFeature)
			{
				return blueprintFeature.HideInCharacterSheetAndLevelUp;
			}
			return false;
		}
		return false;
	}

	public static bool HasPrerequisiteFooter(CalculatedPrerequisite prerequisite, RankEntrySelectionVM selection)
	{
		if (!Game.Instance.IsControllerMouse)
		{
			return false;
		}
		List<CalculatedPrerequisiteFact> list = new List<CalculatedPrerequisiteFact>();
		GetAllPrerequisiteFacts(prerequisite, list);
		return list.Select((CalculatedPrerequisiteFact p) => p.Fact as BlueprintFeature).Distinct().ToList()
			.Any((BlueprintFeature blueprintFeature) => selection.ContainsFeature(blueprintFeature.AssetGuid));
	}

	private static void GetAllPrerequisiteFacts(CalculatedPrerequisite prerequisite, List<CalculatedPrerequisiteFact> facts)
	{
		if (!(prerequisite is CalculatedPrerequisiteFact item))
		{
			if (!(prerequisite is CalculatedPrerequisiteComposite { Prerequisites: var prerequisites }))
			{
				return;
			}
			{
				foreach (CalculatedPrerequisite item2 in prerequisites)
				{
					GetAllPrerequisiteFacts(item2, facts);
				}
				return;
			}
		}
		facts.Add(item);
	}
}
