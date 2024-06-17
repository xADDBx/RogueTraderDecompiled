using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("0436e1194e6a558438138ceca9172e5f")]
public class HasEnoughMoneyForRespec : Condition
{
	protected override string GetConditionCaption()
	{
		return "Has enough money for respec";
	}

	protected override bool CheckCondition()
	{
		return (float)Game.Instance.Player.GetMinimumRespecCost() <= Game.Instance.Player.ProfitFactor.Total;
	}
}
