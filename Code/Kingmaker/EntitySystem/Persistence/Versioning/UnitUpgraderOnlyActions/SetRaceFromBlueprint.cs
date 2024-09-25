using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.EntitySystem.Persistence.Versioning.UnitUpgraderOnlyActions;

[TypeId("2520925018e943008e4bdebf980cce1e")]
public class SetRaceFromBlueprint : UnitUpgraderOnlyAction
{
	public override string GetCaption()
	{
		return "Set race from blueprint";
	}

	protected override void RunActionOverride()
	{
		if (base.Target.Progression.Race != base.Target.OriginalBlueprint.Race)
		{
			base.Target.Progression.SetRace(base.Target.OriginalBlueprint.Race);
		}
	}
}
