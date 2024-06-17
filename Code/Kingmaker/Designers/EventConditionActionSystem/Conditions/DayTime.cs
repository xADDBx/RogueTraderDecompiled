using Kingmaker.AreaLogic.TimeOfDay;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("462d8aa921e95dd4087864b2ef179658")]
public class DayTime : Condition
{
	public TimeOfDay Time;

	protected override string GetConditionCaption()
	{
		return $"Day time is {Time}";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.TimeOfDay == Time;
	}
}
