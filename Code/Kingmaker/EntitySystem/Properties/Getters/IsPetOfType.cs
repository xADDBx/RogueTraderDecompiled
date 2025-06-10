using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("e938cfdde3224b45903392a0eb283cb6")]
public class IsPetOfType : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PetType PetType;

	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		BaseUnitEntity baseUnitEntity = (BaseUnitEntity)this.GetTargetByType(Target);
		if (baseUnitEntity == null)
		{
			throw new Exception($"HasFactGetter: can't find suitable target of type {Target}");
		}
		if (!baseUnitEntity.IsPet)
		{
			return 0;
		}
		if (baseUnitEntity.Master.GetOptional<UnitPartPetOwner>().PetType != PetType)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Check if {Target.Colorized()} is {PetType}";
	}
}
