using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("875f809ba794447a8040d573e71a72e9")]
public class IsPetOfCasterGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		if ((((BaseUnitEntity)this.GetTargetByType(Target)) ?? throw new Exception($"IsPetOfCasterGetter: can't find suitable target of type {Target}")).Master != this.GetTargetByType(PropertyTargetType.ContextCaster))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if " + Target.Colorized() + " is pet of context caster";
	}
}
