using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("d1894c0e31554e6383ded946715ef7e0")]
public class AbilityKeepPreviousAoEPatternOnUi : BlueprintComponent, IAbilityShouldKeepPreviousAoePattern
{
	public bool ShouldKeepAoePattern => true;
}
