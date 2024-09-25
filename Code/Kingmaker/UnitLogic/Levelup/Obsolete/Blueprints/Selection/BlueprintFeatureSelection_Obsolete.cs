using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;

[TypeId("ea44b1ffb0675104cb178df6459f3a21")]
public class BlueprintFeatureSelection_Obsolete : BlueprintFeature, IFeatureSelection
{
	public bool IgnorePrerequisites;

	public bool Obligatory;

	public SelectionMode Mode;

	public FeatureGroup Group;

	public FeatureGroup Group2;

	[ShowIf("IsGroupNone")]
	[SerializeField]
	[FormerlySerializedAs("Features")]
	private BlueprintFeatureReference[] m_Features = new BlueprintFeatureReference[0];

	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("AllFeatures")]
	private BlueprintFeatureReference[] m_AllFeatures = new BlueprintFeatureReference[0];

	public ReferenceArrayProxy<BlueprintFeature> Features
	{
		get
		{
			BlueprintReference<BlueprintFeature>[] features = m_Features;
			return features;
		}
	}

	public ReferenceArrayProxy<BlueprintFeature> AllFeatures
	{
		get
		{
			BlueprintReference<BlueprintFeature>[] allFeatures = m_AllFeatures;
			return allFeatures;
		}
	}

	[UsedImplicitly]
	private bool IsGroupNone => Group == FeatureGroup.None;

	public IEnumerable<IFeatureSelectionItem> GetItems(BaseUnitEntity beforeLevelUpUnit, BaseUnitEntity previewUnit, BlueprintCharacterClass @class)
	{
		if (!IsSelectionProhibited(beforeLevelUpUnit))
		{
			return AllFeatures.Where((BlueprintFeature f) => f != null && !f.IsDlcRestricted());
		}
		return new List<IFeatureSelectionItem>();
	}

	public bool CanSelect(LevelUpState state, FeatureSelectionState selectionState, IFeatureSelectionItem item)
	{
		if (item == null)
		{
			return false;
		}
		BlueprintFeature feature = item.Feature;
		if (feature is BlueprintLevelUpPlanFeaturesList blueprintLevelUpPlanFeaturesList)
		{
			if (!blueprintLevelUpPlanFeaturesList.IsSuitableForSelection(this))
			{
				return false;
			}
		}
		else if (!AllFeatures.Contains(feature))
		{
			return false;
		}
		if (!selectionState.IgnorePrerequisites && !feature.MeetsPrerequisites(state, fromProgression: false, selectionState))
		{
			return false;
		}
		int rank = state.PreviewUnit.Progression.Features.GetRank(feature);
		if (!(feature is IFeatureSelection) && rank >= feature.Ranks)
		{
			return false;
		}
		if (Mode == SelectionMode.OnlyNew && rank > 0)
		{
			return false;
		}
		if (Mode == SelectionMode.OnlyRankUp && rank <= 0)
		{
			return false;
		}
		if (NestedFeatureSelectionUtils.AllNestedFeaturesUnavailable(state, selectionState, feature))
		{
			return false;
		}
		return true;
	}

	public FeatureGroup GetGroup()
	{
		return Group;
	}

	public bool IsIgnorePrerequisites()
	{
		return IgnorePrerequisites;
	}

	public bool IsObligatory()
	{
		return Obligatory;
	}

	public bool IsSelectionProhibited(BaseUnitEntity unit)
	{
		NoSelectionIfAlreadyHasFeature component = this.GetComponent<NoSelectionIfAlreadyHasFeature>();
		if (component == null)
		{
			return false;
		}
		if (component.AnyFeatureFromSelection)
		{
			foreach (BlueprintFeature allFeature in AllFeatures)
			{
				if (unit.Progression.Features.Contains(allFeature))
				{
					return true;
				}
			}
		}
		foreach (BlueprintFeature feature in component.Features)
		{
			if (unit.Progression.Features.Contains(feature))
			{
				return true;
			}
		}
		return false;
	}
}
