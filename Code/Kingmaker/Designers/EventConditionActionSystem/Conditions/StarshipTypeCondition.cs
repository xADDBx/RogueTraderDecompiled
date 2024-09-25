using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("5746726ed56046efb018aaceb7c7bcb1")]
public class StarshipTypeCondition : Condition
{
	public PlayerShipType StarshipType;

	protected override string GetConditionCaption()
	{
		return "Players starship is " + StarshipType.ToString() + " type";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.Player.PlayerShip.Blueprint.ShipType == StarshipType;
	}
}
