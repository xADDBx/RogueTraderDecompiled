using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("7e2fe92f8f828b64dacbc8384bf9061b")]
public class HasEnoughMoneyForCustomCompanion : Condition
{
	protected override string GetConditionCaption()
	{
		return "Has enough money for custom companion";
	}

	protected override bool CheckCondition()
	{
		return (float)Game.Instance.Player.GetCustomCompanionCost() <= Game.Instance.Player.ProfitFactor.Total;
	}
}
