using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[TypeId("68438b4c1569469798532d2c99df4428")]
public class ResetMinDifficulty : PlayerUpgraderOnlyAction
{
	public override string GetCaption()
	{
		return "Reset min difficulty";
	}

	protected override void RunActionOverride()
	{
		Game.Instance.Player.MinDifficultyController.ResetMinDifficulty();
	}
}
