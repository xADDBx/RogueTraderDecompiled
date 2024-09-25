using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UnitLogic.Levelup.Selections.Feature;

public readonly struct FeatureSelectionItem
{
	[NotNull]
	private readonly AddFeaturesToLevelUp m_Source;

	[NotNull]
	public readonly BlueprintFeature Feature;

	public readonly int MaxRank;

	public BlueprintScriptableObject SourceBlueprint => m_Source.OwnerBlueprint;

	public FeatureSelectionItem([NotNull] BlueprintFeature feature, [NotNull] AddFeaturesToLevelUp source, int maxRank)
	{
		m_Source = source;
		Feature = feature;
		MaxRank = maxRank;
	}

	public bool MeetRankPrerequisites(BaseUnitEntity unit)
	{
		int rank = unit.Progression.GetRank(Feature);
		if (MaxRank >= 1 || rank >= 1)
		{
			return MaxRank > rank;
		}
		return true;
	}

	public FeatureGroup GetSourceFeatureGroup()
	{
		return m_Source.Group;
	}

	public bool Equals(FeatureSelectionItem other)
	{
		if (m_Source.Equals(other.m_Source) && Feature.Equals(other.Feature))
		{
			return MaxRank == other.MaxRank;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is FeatureSelectionItem other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(m_Source, Feature, MaxRank);
	}

	public static bool operator ==(FeatureSelectionItem i1, FeatureSelectionItem i2)
	{
		return i1.Equals(i2);
	}

	public static bool operator !=(FeatureSelectionItem i1, FeatureSelectionItem i2)
	{
		return !i1.Equals(i2);
	}
}
