using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Levelup.Obsolete;

namespace Kingmaker.Designers.Mechanics.Recommendations;

[TypeId("db645d8b907cba540a36958fb9b43254")]
public abstract class LevelUpRecommendationComponent : BlueprintComponent
{
	public abstract RecommendationPriority GetPriority([CanBeNull] LevelUpState levelUpState);
}
