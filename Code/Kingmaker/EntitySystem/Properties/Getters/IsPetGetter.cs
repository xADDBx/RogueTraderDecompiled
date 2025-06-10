using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("c40b4ede65fb4807af5f252e80a558cf")]
public class IsPetGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		if (!(((BaseUnitEntity)this.GetTargetByType(Target)) ?? throw new Exception($"HasFactGetter: can't find suitable target of type {Target}")).IsPet)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if " + Target.Colorized() + " is pet";
	}
}
