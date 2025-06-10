using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("a4e358624b664aecb091c11b8b1801d1")]
public class IsMasterOfTargetGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		if ((((BaseUnitEntity)this.GetTargetByType(Target)) ?? throw new Exception($"IsMasterOfTargetGetter: can't find suitable target of type {Target}")).GetOptional<UnitPartPetOwner>()?.PetUnit != this.GetTargetByType(PropertyTargetType.CurrentTarget))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if " + Target.Colorized() + " is pet of context target";
	}
}
