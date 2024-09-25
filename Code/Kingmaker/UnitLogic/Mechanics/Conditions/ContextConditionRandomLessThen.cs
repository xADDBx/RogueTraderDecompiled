using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.Random;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("12b774374ac6829459deaf2f61048967")]
public class ContextConditionRandomLessThen : ContextCondition
{
	public ContextValue Value;

	protected override string GetConditionCaption()
	{
		return $"Check if d100 less or equal than {Value}";
	}

	protected override bool CheckCondition()
	{
		return PFStatefulRandom.Mechanics.Range(1, 100) <= Value.Calculate(base.Context);
	}
}
