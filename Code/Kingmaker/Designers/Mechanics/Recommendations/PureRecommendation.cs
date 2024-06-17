using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Designers.Mechanics.Recommendations;

[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintFeature))]
[TypeId("0596deabaca35e541a675eddb6a84e53")]
public class PureRecommendation : LevelUpRecommendationComponent
{
	public RecommendationPriority Priority;

	public override RecommendationPriority GetPriority([CanBeNull] LevelUpState levelUpState)
	{
		return Priority;
	}
}
