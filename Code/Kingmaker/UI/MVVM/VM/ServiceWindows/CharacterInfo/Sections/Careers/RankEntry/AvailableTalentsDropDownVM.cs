using System.Collections.Generic;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;

public class AvailableTalentsDropDownVM : VirtualListElementVMBase
{
	public FeaturesFilter.FeatureFilterType FilterType { get; }

	public string Title { get; }

	public BoolReactiveProperty IsExpanded { get; } = new BoolReactiveProperty();


	public int FeatureCount { get; set; }

	public List<VirtualListElementVMBase> GroupedFeatures { get; set; }

	public AvailableTalentsDropDownVM(FeaturesFilter.FeatureFilterType filterType, string title, bool expanded = false)
	{
		FilterType = filterType;
		Title = title;
		IsExpanded.Value = expanded;
	}

	public void Switch()
	{
		IsExpanded.Value = !IsExpanded.Value;
		SetGroupedFeaturesActive(IsExpanded.Value);
	}

	public void Expand()
	{
		IsExpanded.Value = true;
		SetGroupedFeaturesActive(active: true);
	}

	public void Collapse()
	{
		IsExpanded.Value = false;
		SetGroupedFeaturesActive(active: false);
	}

	private void SetGroupedFeaturesActive(bool active)
	{
		if (GroupedFeatures == null)
		{
			return;
		}
		foreach (VirtualListElementVMBase groupedFeature in GroupedFeatures)
		{
			if (!active && groupedFeature is BaseRankEntryFeatureVM { IsFavorite: not false })
			{
				groupedFeature.Active.Value = true;
			}
			else
			{
				groupedFeature.Active.Value = active;
			}
		}
	}

	protected override void DisposeImplementation()
	{
	}
}
