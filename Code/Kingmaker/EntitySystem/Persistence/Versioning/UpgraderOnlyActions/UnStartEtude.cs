using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.EntitySystem.Persistence.Versioning.UpgraderOnlyActions;

[TypeId("9834963f1a257bc4e9134d8ed2ffe2c2")]
internal class UnStartEtude : PlayerUpgraderOnlyAction
{
	public BlueprintEtudeReference Etude;

	protected override void RunActionOverride()
	{
		BlueprintEtude blueprintEtude = Etude.Get();
		if ((bool)blueprintEtude && !Game.Instance.Player.EtudesSystem.EtudeIsNotStarted(blueprintEtude))
		{
			Game.Instance.Player.EtudesSystem.UnstartEtude(blueprintEtude);
		}
	}

	public override string GetCaption()
	{
		return "Unstart " + Etude;
	}

	public override string GetDescription()
	{
		return "Unstarts " + Etude.NameSafe() + " making as if it had never triggered";
	}
}
