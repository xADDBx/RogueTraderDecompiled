using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Achievements;

[PlayerUpgraderAllowed(false)]
[TypeId("986015feb8a90774aba3ad6838fadd4f")]
public class ActionAchievementIncrementCounter : GameAction
{
	[SerializeField]
	[FormerlySerializedAs("Achievement")]
	private AchievementDataReference m_Achievement;

	public AchievementData Achievement => m_Achievement?.Get();

	public override string GetCaption()
	{
		return "Increment achievement counter: " + Achievement?.name;
	}

	protected override void RunAction()
	{
		if (Achievement != null)
		{
			Game.Instance.Player.Achievements.IncrementCounter(Achievement);
		}
	}
}
