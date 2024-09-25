using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Designers.Mechanics.Recommendations;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintFeature))]
[TypeId("eb8c8df95f51c1c4fa74733c31a853ee")]
public class RecommendationStatMiminum : LevelUpRecommendationComponent
{
	public StatType Stat;

	public int MinimalValue;

	public bool GoodIfHigher;

	public override RecommendationPriority GetPriority([CanBeNull] LevelUpState levelUpState)
	{
		if (levelUpState == null)
		{
			return RecommendationPriority.Same;
		}
		if (levelUpState.PreviewUnit.Stats.GetStat(Stat).PermanentValue >= MinimalValue)
		{
			if (!GoodIfHigher)
			{
				return RecommendationPriority.Same;
			}
			return RecommendationPriority.Good;
		}
		return RecommendationPriority.Bad;
	}
}
