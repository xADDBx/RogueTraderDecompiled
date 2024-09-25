using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[TypeId("ffc2a522332a4f3cbc4a8ba61044afee")]
public class ClearInitialPositionsForMovableAreaController : PlayerUpgraderOnlyAction
{
	public override string GetCaption()
	{
		return "Clear initial positions for movable area controller";
	}

	protected override void RunActionOverride()
	{
		Game.Instance?.UnitMovableAreaController?.ClearInitialPositions();
	}
}
