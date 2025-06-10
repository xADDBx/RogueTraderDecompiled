using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("71183f25eb1140ebbd602153ffe388d8")]
public class IsPetOfTargetGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		if ((((BaseUnitEntity)this.GetTargetByType(Target)) ?? throw new Exception($"IsPetOfTargetGetter: can't find suitable target of type {Target}")).Master != this.GetTargetByType(PropertyTargetType.CurrentTarget))
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
