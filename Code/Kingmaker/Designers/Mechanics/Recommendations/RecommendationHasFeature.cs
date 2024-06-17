using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Recommendations;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintFeature))]
[TypeId("e1040f75a72eccb44927903b586b3171")]
public class RecommendationHasFeature : LevelUpRecommendationComponent
{
	[SerializeField]
	[FormerlySerializedAs("Feature")]
	private BlueprintUnitFactReference m_Feature;

	public bool Mandatory;

	public BlueprintUnitFact Feature => m_Feature?.Get();

	public override RecommendationPriority GetPriority([CanBeNull] LevelUpState levelUpState)
	{
		if (levelUpState == null)
		{
			return RecommendationPriority.Same;
		}
		if (!levelUpState.PreviewUnit.Facts.Contains(Feature))
		{
			if (!Mandatory)
			{
				return RecommendationPriority.Same;
			}
			return RecommendationPriority.Bad;
		}
		if (!Mandatory)
		{
			return RecommendationPriority.Good;
		}
		return RecommendationPriority.Same;
	}
}
