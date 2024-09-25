using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Squads;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("7891081dede847f2a2a54825a3a8fdb7")]
public class CheckIsUnitInSquadGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		if (this.GetTargetByType(Target).GetOptional<PartSquad>() == null)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return Target.Colorized() + " is in squad";
	}
}
