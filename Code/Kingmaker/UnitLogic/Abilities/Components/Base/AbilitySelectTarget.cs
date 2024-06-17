using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("4d05e8ef635fa4445b63ac53e2feafe9")]
public abstract class AbilitySelectTarget : BlueprintComponent
{
	public abstract IEnumerable<TargetWrapper> Select(AbilityExecutionContext context, TargetWrapper anchor);

	public virtual float? GetSpreadSpeedMps()
	{
		return null;
	}
}
