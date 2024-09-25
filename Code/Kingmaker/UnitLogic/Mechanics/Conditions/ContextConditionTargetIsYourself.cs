using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("1bb7cd34a36c43bca61048eb36ae8d4b")]
public class ContextConditionTargetIsYourself : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return "Target is the caster himself";
	}

	protected override bool CheckCondition()
	{
		if (base.Target.Entity == null)
		{
			PFLog.Default.Error("Target unit is missing");
			return false;
		}
		return base.Target.Entity == base.Context.MaybeCaster;
	}
}
