using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("9d0854efac00480daafcb3ee92a64553")]
public class IsPetOwnerOfTypeGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PetType PetType;

	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		UnitPartPetOwner optional = (((BaseUnitEntity)this.GetTargetByType(Target)) ?? throw new Exception($"HasFactGetter: can't find suitable target of type {Target}")).Parts.GetOptional<UnitPartPetOwner>();
		if (optional == null)
		{
			return 0;
		}
		if (optional.PetType != PetType)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Check if {Target.Colorized()} is owner of {PetType}";
	}
}
