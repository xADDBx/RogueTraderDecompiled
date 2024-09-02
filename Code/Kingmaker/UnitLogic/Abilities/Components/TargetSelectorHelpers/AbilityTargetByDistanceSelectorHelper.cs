using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetSelectorHelpers;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("6c1ce77a3a474f5d9c6aa76b1d24b558")]
public class AbilityTargetByDistanceSelectorHelper : BlueprintComponent
{
}
