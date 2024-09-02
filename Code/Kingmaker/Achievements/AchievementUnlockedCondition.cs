using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Achievements;

[TypeId("261fe9afdbb743b0859b1a03dfcd6097")]
public class AchievementUnlockedCondition : BlueprintComponent
{
	[SerializeField]
	[ValidateNotNull]
	private AchievementDataReference m_AchievementDataReference;

	private AchievementData AchievementData => m_AchievementDataReference;

	public bool IsUnlocked()
	{
		return Game.Instance.Player.Achievements.IsAchievementUnlocked(AchievementData);
	}
}
