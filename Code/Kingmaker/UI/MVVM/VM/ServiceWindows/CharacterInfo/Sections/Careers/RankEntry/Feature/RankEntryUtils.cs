using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
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
}
