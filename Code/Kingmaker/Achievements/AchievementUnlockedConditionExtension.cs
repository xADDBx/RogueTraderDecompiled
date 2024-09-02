using Kingmaker.Blueprints;

namespace Kingmaker.Achievements;

public static class AchievementUnlockedConditionExtension
{
	public static bool IsAchievementUnlocked(this BlueprintScriptableObject blueprint)
	{
		if (blueprint != null)
		{
			return !(blueprint.GetComponent<AchievementUnlockedCondition>()?.IsUnlocked() ?? false);
		}
		return false;
	}
}
