using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("18b4005ab45ded44dbacab84cab0b247")]
public class ContextConditionIsMainTarget : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return "Is main target";
	}

	protected override bool CheckCondition()
	{
		return base.Context.MainTarget.Equals(base.Target);
	}
}
