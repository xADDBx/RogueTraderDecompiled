using System;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UI.Common;

public static class FeaturesFilter
{
	public enum FeatureFilterType
	{
		None,
		RecommendedFilter,
		FavoritesFilter,
		OffenseFilter,
		DefenseFilter,
		SupportFilter,
		UniversalFilter,
		ArchetypeFilter,
		OriginFilter,
		WarpFilter
	}

	public static BlueprintFeature.FeatureType? GetType(FeatureFilterType type)
	{
		return type switch
		{
			FeatureFilterType.None => null, 
			FeatureFilterType.RecommendedFilter => null, 
			FeatureFilterType.FavoritesFilter => null, 
			FeatureFilterType.OffenseFilter => BlueprintFeature.FeatureType.Offense, 
			FeatureFilterType.DefenseFilter => BlueprintFeature.FeatureType.Defense, 
			FeatureFilterType.SupportFilter => BlueprintFeature.FeatureType.Support, 
			FeatureFilterType.UniversalFilter => BlueprintFeature.FeatureType.Universal, 
			FeatureFilterType.ArchetypeFilter => BlueprintFeature.FeatureType.Archetype, 
			FeatureFilterType.OriginFilter => BlueprintFeature.FeatureType.Origin, 
			FeatureFilterType.WarpFilter => BlueprintFeature.FeatureType.Warp, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public static bool MeetsFilter(this BlueprintFeature feature, FeatureFilterType filterType)
	{
		if (filterType == FeatureFilterType.None || filterType == FeatureFilterType.RecommendedFilter || filterType == FeatureFilterType.FavoritesFilter)
		{
			return true;
		}
		BlueprintFeature.FeatureType? type = GetType(filterType);
		if (type.HasValue)
		{
			return feature.FeatureTypes.Contains(type.Value);
		}
		return false;
	}
}
