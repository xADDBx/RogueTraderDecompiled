using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("b0c4370c781db0142b035f14ca13a6a5")]
public class ContextConditionIsEnemy : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return "Check if target is an enemy";
	}

	protected override bool CheckCondition()
	{
		if (base.Context.MaybeCaster == null)
		{
			PFLog.Default.Error(this, "Caster is missing");
			return false;
		}
		return base.Target.Entity.IsEnemy(base.Context.MaybeCaster);
	}
}
