using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;

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
		MechanicEntity entity = base.Target.Entity;
		if (entity == null)
		{
			PFLog.Default.Error("Target unit is missing");
			return false;
		}
		return entity.IsEnemy(base.Context.MaybeCaster);
	}
}
