using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("65bd45f065b74ef493bbb36e660500ad")]
public class ChangeNewPassageCost : GameAction
{
	public int NewCost;

	public override string GetCaption()
	{
		return "Change cost of creating new passage";
	}

	protected override void RunAction()
	{
		Game.Instance.Player.WarpTravelState.CreateNewPassageCost = NewCost;
	}
}
