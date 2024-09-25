using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("4d09aeabe1794694b3fb92c65dc96843")]
public class DayOfTheMonth : Condition
{
	public int Day;

	protected override string GetConditionCaption()
	{
		return $"Day of month is {Day}";
	}

	protected override bool CheckCondition()
	{
		return (Game.Instance.BlueprintRoot.Calendar.GetStartDate() + Game.Instance.TimeController.GameTime).Day == Day;
	}
}
