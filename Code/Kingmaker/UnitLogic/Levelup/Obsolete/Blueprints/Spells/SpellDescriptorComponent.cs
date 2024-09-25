using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;

[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintAbilityAreaEffect))]
[TypeId("91216784d99a12e428bf782c8a8c7f5c")]
public class SpellDescriptorComponent : BlueprintComponent
{
	public SpellDescriptorWrapper Descriptor;
}
