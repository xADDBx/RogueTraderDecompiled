using System;

namespace Kingmaker.Achievements;

public interface IPlatformAchievementHandler : IDisposable
{
	void OnAchievementUnlocked(AchievementEntity achievementEntity);

	void OnAchievementProgressUpdated(AchievementEntity achievementEntity);
}
