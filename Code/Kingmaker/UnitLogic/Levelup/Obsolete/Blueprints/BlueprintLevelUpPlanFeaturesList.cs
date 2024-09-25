using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;

[TypeId("6d13b4e7f6894bd78c34d28737f5bc18")]
public class BlueprintLevelUpPlanFeaturesList : BlueprintFeature
{
	[Serializable]
	public class FeatureWrapper
	{
		[SerializeField]
		private BlueprintFeatureReference m_Feature;

		public BlueprintFeature Feature => m_Feature;

		public FeatureParam Param => null;
	}

	public FeatureWrapper[] Features;

	[CanBeNull]
	public FeatureWrapper GetSuitableFeature(BaseUnitEntity unit, [CanBeNull] FeatureSelectionState selectionState, [CanBeNull] LevelUpState levelupState, bool fromProgression)
	{
		FeatureWrapper[] features = Features;
		foreach (FeatureWrapper featureWrapper in features)
		{
			if (unit.Progression.Features.Get(featureWrapper.Feature, featureWrapper.Param) == null && featureWrapper.Feature.MeetsPrerequisites(unit, fromProgression, selectionState, levelupState))
			{
				return featureWrapper;
			}
		}
		return null;
	}

	public override bool MeetsPrerequisites(BaseUnitEntity unit, bool fromProgression, FeatureSelectionState selectionState, LevelUpState state)
	{
		FeatureWrapper[] features = Features;
		for (int i = 0; i < features.Length; i++)
		{
			if (features[i].Feature.MeetsPrerequisites(unit, fromProgression, selectionState, state))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSuitableForSelection(BlueprintFeatureSelection_Obsolete selection)
	{
		FeatureWrapper[] features = Features;
		foreach (FeatureWrapper featureWrapper in features)
		{
			if (!selection.AllFeatures.HasReference(featureWrapper.Feature))
			{
				return false;
			}
		}
		return true;
	}
}
