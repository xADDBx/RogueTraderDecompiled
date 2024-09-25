using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Exploration;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("b2052f518577440783677317663cac36")]
public class AnomalyShipTemper : Condition
{
	public AnomalyStarShip.ShipTemper Temper;

	protected override string GetConditionCaption()
	{
		return "Check temper of currently interacted anomaly";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.Player.StarSystemsState.StarSystemContextData.StarSystemObject?.Blueprint.GetComponent<AnomalyStarShip>()?.Temper == Temper;
	}
}
