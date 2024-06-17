using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("26455fc841ec4aee9d6a1f68bad45cfc")]
public class WarhammerConcentrationAbility : BlueprintComponent
{
	public int ActionPointCost;

	public int MovementPointCost;
}
