using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("f188554c9c0b43218a8b430182dbca12")]
public class IsMasterOfCasterGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		if ((((BaseUnitEntity)this.GetTargetByType(Target)) ?? throw new Exception($"IsMasterOfCasterGetter: can't find suitable target of type {Target}")).GetOptional<UnitPartPetOwner>()?.PetUnit != this.GetTargetByType(PropertyTargetType.ContextCaster))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if " + Target.Colorized() + " is master of context caster";
	}
}
