using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.EntitySystem.Persistence.Versioning.UnitUpgraderOnlyActions;

[TypeId("bde26a3c1b374e6f96b3ce4e35188796")]
public class SetAlignmentFromBlueprint : UnitUpgraderOnlyAction
{
	public override string GetCaption()
	{
		return "Set alignment from blueprint";
	}

	protected override void RunActionOverride()
	{
		base.Target.Alignment.Set(base.Target.OriginalBlueprint.Alignment);
	}
}
