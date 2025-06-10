using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("90d13818ca8b44a983587db1a04b3980")]
public class ContextConditionIsBig : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return "";
	}

	protected override bool CheckCondition()
	{
		return base.Target.Entity?.Size.IsBigAndEvenUnit() ?? false;
	}
}
