using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("e2beab7835754e42b16177b95d1e3b12")]
public class AbilityCanTargetDeadUnits : BlueprintComponent, ICanTargetDeadUnits
{
}
