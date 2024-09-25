using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("c5b3b6002404ca249add2943e99f366a")]
public class ContextConditionIsAlly : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return "Is ally";
	}

	protected override bool CheckCondition()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		if (maybeCaster == null)
		{
			PFLog.Default.Error("Caster is missing");
			return false;
		}
		MechanicEntity entity = base.Target.Entity;
		if (entity == null)
		{
			PFLog.Default.Error("Target unit is missing");
			return false;
		}
		return maybeCaster.IsAlly(entity);
	}
}
