using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("5e9de6e6bdbd4798acad94fb66de81b0")]
public class ContextConditionIsMasterOfCaster : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return string.Concat("Check if target is master of caster");
	}

	protected override bool CheckCondition()
	{
		BaseUnitEntity baseUnitEntity = (base.Context.MaybeCaster as BaseUnitEntity)?.Master;
		if (baseUnitEntity == null)
		{
			PFLog.Actions.Error("ContextConditionIsMasterOfCaster called from caster without a master");
			return false;
		}
		return base.Context.MainTarget.Entity == baseUnitEntity;
	}
}
