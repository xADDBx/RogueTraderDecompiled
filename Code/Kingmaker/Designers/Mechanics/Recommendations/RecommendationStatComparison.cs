using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Designers.Mechanics.Recommendations;

[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintFeature))]
[TypeId("da44814f2959dc14da3fc1d71b68027d")]
public class RecommendationStatComparison : LevelUpRecommendationComponent
{
	public StatType HigherStat;

	public StatType LowerStat;

	public int Diff;

	public override RecommendationPriority GetPriority([CanBeNull] LevelUpState levelUpState)
	{
		if (levelUpState == null)
		{
			return RecommendationPriority.Same;
		}
		ModifiableValue stat = levelUpState.PreviewUnit.Stats.GetStat(HigherStat);
		ModifiableValue stat2 = levelUpState.PreviewUnit.Stats.GetStat(LowerStat);
		if (stat == null || stat2 == null)
		{
			return RecommendationPriority.Same;
		}
		if (stat.PermanentValue - Diff > stat2.PermanentValue)
		{
			return RecommendationPriority.Good;
		}
		if (stat.PermanentValue >= stat2.PermanentValue)
		{
			return RecommendationPriority.Irrelevant;
		}
		return RecommendationPriority.Bad;
	}
}
