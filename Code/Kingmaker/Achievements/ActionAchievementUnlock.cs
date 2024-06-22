using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Achievements;

[PlayerUpgraderAllowed(false)]
[TypeId("597bbe9e03eb8fd4eafc4a740a217f1c")]
public class ActionAchievementUnlock : GameAction
{
	[SerializeField]
	[FormerlySerializedAs("Achievement")]
	private AchievementDataReference m_Achievement;

	public AchievementData Achievement => m_Achievement?.Get();

	public override string GetCaption()
	{
		return "Unlock achievement " + Achievement.name;
	}

	protected override void RunAction()
	{
		Game.Instance.Player.Achievements.Unlock(Achievement);
	}
}
