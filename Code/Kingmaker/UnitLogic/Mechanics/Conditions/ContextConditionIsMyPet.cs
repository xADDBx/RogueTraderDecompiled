using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("70a0a20368a64726a77d12fbc6abb1e1")]
public class ContextConditionIsMyPet : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return "Target is my pet?";
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
		if (baseUnitEntity2.IsPet)
		{
			return baseUnitEntity2.Master == baseUnitEntity;
		}
		return false;
	}
}
