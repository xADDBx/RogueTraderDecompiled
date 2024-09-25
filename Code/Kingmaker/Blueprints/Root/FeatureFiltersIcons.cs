using System;
using Kingmaker.UI.Common;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class FeatureFiltersIcons
{
	public Sprite NoneIcon;

	public Sprite RecommendedFilterIcon;

	public Sprite FavoritesFilterIcon;

	public Sprite OffenseFilterIcon;

	public Sprite DefenseFilterIcon;

	public Sprite SupportFilterIcon;

	public Sprite UniversalFilterIcon;

	public Sprite ArchetypeFilterIcon;

	public Sprite OriginFilterIcon;

	public Sprite WarpFilterIcon;

	public Sprite GetIconFor(FeaturesFilter.FeatureFilterType filter)
	{
		return filter switch
		{
			FeaturesFilter.FeatureFilterType.None => NoneIcon, 
			FeaturesFilter.FeatureFilterType.RecommendedFilter => RecommendedFilterIcon, 
			FeaturesFilter.FeatureFilterType.FavoritesFilter => FavoritesFilterIcon, 
			FeaturesFilter.FeatureFilterType.OffenseFilter => OffenseFilterIcon, 
			FeaturesFilter.FeatureFilterType.DefenseFilter => DefenseFilterIcon, 
			FeaturesFilter.FeatureFilterType.SupportFilter => SupportFilterIcon, 
			FeaturesFilter.FeatureFilterType.UniversalFilter => UniversalFilterIcon, 
			FeaturesFilter.FeatureFilterType.ArchetypeFilter => ArchetypeFilterIcon, 
			FeaturesFilter.FeatureFilterType.OriginFilter => OriginFilterIcon, 
			FeaturesFilter.FeatureFilterType.WarpFilter => WarpFilterIcon, 
			_ => throw new ArgumentOutOfRangeException("filter", filter, null), 
		};
	}
}
