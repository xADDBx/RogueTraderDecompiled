using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("19f5a7a3459f1bb43bcae3b22a893e5c")]
public class MonthFromList : Condition
{
	public int[] Months;

	protected override string GetConditionCaption()
	{
		return $"Month number is in list";
	}

	protected override bool CheckCondition()
	{
		return Months.Contains((Game.Instance.BlueprintRoot.Calendar.GetStartDate() + Game.Instance.TimeController.GameTime).Month);
	}
}
