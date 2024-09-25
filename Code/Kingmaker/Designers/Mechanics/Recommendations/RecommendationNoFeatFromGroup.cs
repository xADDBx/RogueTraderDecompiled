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
[TypeId("4b6c2ac95f357d845ae3a30d6fc6641a")]
public class RecommendationNoFeatFromGroup : LevelUpRecommendationComponent
{
	[SerializeField]
	[FormerlySerializedAs("Features")]
	private BlueprintUnitFactReference[] m_Features;

	public bool GoodIfNoFeature;

	public ReferenceArrayProxy<BlueprintUnitFact> Features
	{
		get
		{
			BlueprintReference<BlueprintUnitFact>[] features = m_Features;
			return features;
		}
	}

	public override RecommendationPriority GetPriority([CanBeNull] LevelUpState levelUpState)
	{
		if (levelUpState == null)
		{
			return RecommendationPriority.Same;
		}
		bool flag = false;
		foreach (BlueprintUnitFact feature in Features)
		{
			if (levelUpState.PreviewUnit.Facts.Contains(feature))
			{
				flag = true;
			}
		}
		if (flag)
		{
			return RecommendationPriority.Bad;
		}
		if (!GoodIfNoFeature)
		{
			return RecommendationPriority.Same;
		}
		return RecommendationPriority.Good;
	}
}
