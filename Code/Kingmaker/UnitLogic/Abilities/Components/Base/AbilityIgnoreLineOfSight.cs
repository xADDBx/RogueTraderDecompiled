using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[Serializable]
[TypeId("939084ae6cfa48999bcf13aea11aa067")]
public class AbilityIgnoreLineOfSight : BlueprintComponent, IAbilityIgnoreLOS
{
	public bool ShouldIgnoreLOS(AbilityData abilityData)
	{
		return true;
	}
}
