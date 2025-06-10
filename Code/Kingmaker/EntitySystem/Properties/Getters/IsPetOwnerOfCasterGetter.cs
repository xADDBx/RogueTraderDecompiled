using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("062ca89ca8c141a58635a7dc67316b26")]
public class IsPetOwnerOfCasterGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		BaseUnitEntity baseUnitEntity = (BaseUnitEntity)base.PropertyContext.ContextCaster;
		if (baseUnitEntity == null || !baseUnitEntity.IsPet)
		{
			throw new Exception("IsPetOwnerOfCasterGetter: check for owner from not a pet");
		}
		if ((((BaseUnitEntity)this.GetTargetByType(Target)) ?? throw new Exception($"IsPetOwnerOfCasterGetter: can't find suitable target of type {Target}")) != baseUnitEntity.Master)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if " + Target.Colorized() + " is caster owner";
	}
}
