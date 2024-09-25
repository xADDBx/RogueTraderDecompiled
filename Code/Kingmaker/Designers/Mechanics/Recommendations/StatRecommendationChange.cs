using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Designers.Mechanics.Recommendations;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintFeature))]
[TypeId("a2ca52e886244b2dbd30a5368665150c")]
public class StatRecommendationChange : BlueprintComponent
{
	public StatType Stat;

	public bool Recommended;
}
