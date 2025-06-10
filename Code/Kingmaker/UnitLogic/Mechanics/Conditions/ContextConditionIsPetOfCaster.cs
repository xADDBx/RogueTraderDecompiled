using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("f2fc4909ae9d4c6284d1e723b5ba95aa")]
public class ContextConditionIsPetOfCaster : ContextCondition
{
	protected override string GetConditionCaption()
	{
		return string.Concat("Check if target is pet of caster");
	}

	protected override bool CheckCondition()
	{
		BaseUnitEntity baseUnitEntity = base.Context.MaybeCaster?.GetOptional<UnitPartPetOwner>()?.PetUnit;
		if (baseUnitEntity == null)
		{
			PFLog.Actions.Error("ContextConditionIsPetOfCaster called from caster without a pet");
			return false;
		}
		return base.Context.MainTarget.Entity == baseUnitEntity;
	}
}
