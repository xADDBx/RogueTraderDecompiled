using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Achievements.Logic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Settings.Difficulty;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Achievements;

public static class AchievementLogicFactory
{
	private static readonly Dictionary<AchievementType, Func<AchievementEntity, AchievementLogic>> Creators = new Dictionary<AchievementType, Func<AchievementEntity, AchievementLogic>> { 
	{
		AchievementType.Flags,
		(AchievementEntity e) => new AchievementLogicUnlockableFlags(e)
	} };

	private static DifficultyPresetAsset CoreDifficulty => BlueprintRoot.Instance.DifficultyList.CoreDifficulty;

	private static DifficultyPresetAsset InsaneDifficulty => BlueprintRoot.Instance.DifficultyList.UnfairDifficulty;

	[CanBeNull]
	public static AchievementLogic Create(AchievementEntity entity)
	{
		return Creators.Get(entity.Data.Type)?.Invoke(entity);
	}
}
