using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("2eacb938afe24a7bae3179c8a0718b46")]
public class ContextConditionIsMyMaster : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return "Target is my master?";
	}

	protected override bool CheckCondition()
	{
		if (!(base.Context.MaybeCaster is BaseUnitEntity baseUnitEntity))
		{
			PFLog.Default.Error("Caster is missing");
			return false;
		}
		if (!(base.Target.Entity is BaseUnitEntity baseUnitEntity2))
		{
			PFLog.Default.Error("Target is missing");
			return false;
		}
		if (baseUnitEntity.IsPet)
		{
			return baseUnitEntity.Master == baseUnitEntity2;
		}
		return false;
	}
}
