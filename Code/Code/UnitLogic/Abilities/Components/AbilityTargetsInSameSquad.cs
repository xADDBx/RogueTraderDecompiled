using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;

namespace Code.UnitLogic.Abilities.Components;

[Serializable]
[TypeId("50c2f826f9c8441687811baf74f33318")]
public class AbilityTargetsInSameSquad : AbilitySelectTarget
{
	public PropertyCalculator TargetCondition;

	public override IEnumerable<TargetWrapper> Select(AbilityExecutionContext context, TargetWrapper anchor)
	{
		PartSquad squadOptional = context.Caster.GetSquadOptional();
		if (squadOptional == null)
		{
			return Enumerable.Empty<TargetWrapper>();
		}
		return from i in squadOptional.Units.Select((UnitReference i) => i.Entity).NotNull()
			where IsSuitable(context, i.ToBaseUnitEntity())
			select new TargetWrapper(i.ToBaseUnitEntity());
	}

	private bool IsSuitable(AbilityExecutionContext context, MechanicEntity target)
	{
		PropertyContext context2 = new PropertyContext(context.Caster, null, target, context, null, context.Ability);
		return TargetCondition.GetValue(context2) != 0;
	}
}
