using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[PlayerUpgraderAllowed(false)]
[TypeId("07b44e4b8c5e4725ad007726c85820cb")]
public class NavigatorResourceEnough : Condition
{
	public int Count;

	protected override string GetConditionCaption()
	{
		return $"Check if player have at least {Count} of navigator resource";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.Player.WarpTravelState.NavigatorResource >= Count;
	}
}
