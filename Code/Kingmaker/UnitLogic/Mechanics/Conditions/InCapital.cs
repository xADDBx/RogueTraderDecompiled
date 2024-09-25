using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("36bc01dee40ec13439040aeb63bbbeaf")]
public class InCapital : Condition
{
	protected override string GetConditionCaption()
	{
		return "In Capital";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.LoadedAreaState.Settings.CapitalPartyMode;
	}
}
