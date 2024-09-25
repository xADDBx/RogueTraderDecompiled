using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.EntitySystem.Persistence.Versioning.UnitUpgraderOnlyActions;

[TypeId("fe01e09c7bf145bfb66e0b0db74c4b89")]
public class SetHandsFromBlueprint : UnitUpgraderOnlyAction
{
	public override string GetCaption()
	{
		return "Set hands from blueprint";
	}

	protected override void RunActionOverride()
	{
		base.Target.Body.UpgradeHandsFromBlueprint();
	}
}
