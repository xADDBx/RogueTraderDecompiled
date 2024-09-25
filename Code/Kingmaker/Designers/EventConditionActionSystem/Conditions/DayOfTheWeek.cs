using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("058cd2184dc84bb18c62f21fa593016b")]
public class DayOfTheWeek : Condition
{
	public DayOfWeek Day;

	protected override string GetConditionCaption()
	{
		return $"Day of week is {Day}";
	}

	protected override bool CheckCondition()
	{
		return (Game.Instance.BlueprintRoot.Calendar.GetStartDate() + Game.Instance.TimeController.GameTime).DayOfWeek == Day;
	}
}
