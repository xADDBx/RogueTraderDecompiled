using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Squads;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("f6c26276e60540519705321a2cfd766a")]
public class CheckUnitIsSquadLeaderGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		PartSquad optional = this.GetTargetByType(Target).GetOptional<PartSquad>();
		if (optional == null || !optional.IsLeader)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return Target.Colorized() + " is squad leader";
	}
}
