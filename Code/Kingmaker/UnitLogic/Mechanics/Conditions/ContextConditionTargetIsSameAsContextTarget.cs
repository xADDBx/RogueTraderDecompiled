using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("e55aef8c6f3068840ac8ffa7ef361691")]
public class ContextConditionTargetIsSameAsContextTarget : ContextCondition
{
	protected override bool CheckCondition()
	{
		if (base.Context.MainTarget.Entity == null)
		{
			PFLog.Default.Error("Context target unit is missing");
			return false;
		}
		if (base.Target.Entity == null)
		{
			PFLog.Default.Error("Target unit is missing");
			return false;
		}
		return base.Target.Entity.Blueprint == base.Context.MainTarget.Entity.Blueprint;
	}

	protected override string GetConditionCaption()
	{
		return "Unit's blueprint has the same LocalizedName as context target's";
	}
}
