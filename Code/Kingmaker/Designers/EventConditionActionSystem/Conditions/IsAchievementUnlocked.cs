using Kingmaker.Achievements;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("746ea8847d59470babff7430826d8f74")]
public class IsAchievementUnlocked : Condition
{
	[SerializeField]
	private AchievementDataReference m_AchievementDataReference;

	private AchievementData AchievementData => m_AchievementDataReference;

	protected override bool CheckCondition()
	{
		return Game.Instance.Player.Achievements.IsAchievementUnlocked(AchievementData);
	}

	protected override string GetConditionCaption()
	{
		return $"{AchievementData} achievement is enabled";
	}
}
