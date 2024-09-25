using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("1dd7af4212bb7a84497b86ac1ba4ded1")]
public class ContextConditionDistanceToTarget : ContextCondition
{
	public int DistanceGreater;

	protected override string GetConditionCaption()
	{
		return $"Check if distance from caster to target is greater than {DistanceGreater}";
	}

	protected override bool CheckCondition()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		if (maybeCaster == null)
		{
			Element.LogError(this, "Caster is missing");
			return false;
		}
		if (base.Target.Entity == null)
		{
			Element.LogError(this, "Target unit is missing");
			return false;
		}
		if (base.Target.Entity == maybeCaster)
		{
			return false;
		}
		return base.Target.Entity.DistanceToInCells(maybeCaster) > DistanceGreater;
	}
}
