using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;

namespace Kingmaker.View.MapObjects;

public static class SkillCheckHelper
{
	public static int GetDC(this SkillCheckDifficulty difficulty)
	{
		if (difficulty == SkillCheckDifficulty.Custom)
		{
			PFLog.Default.Error("Difficulty is Custom");
			return 0;
		}
		BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
		BlueprintArea previousVisitedArea = Game.Instance.Player.PreviousVisitedArea;
		BlueprintArea blueprintArea = ((!currentlyLoadedArea.IsGlobalmapArea) ? currentlyLoadedArea : (previousVisitedArea.IsGlobalmapArea ? previousVisitedArea : null));
		if (blueprintArea == null)
		{
			PFLog.Default.Error("Area is missing");
			return 0;
		}
		return Root.WH.SkillCheckRoot.GetSkillCheckDC(difficulty, blueprintArea.GetCR());
	}
}
