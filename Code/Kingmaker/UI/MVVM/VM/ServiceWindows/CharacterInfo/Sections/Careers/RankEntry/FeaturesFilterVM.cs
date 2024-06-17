using System;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;

public class FeaturesFilterVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<FeaturesFilter.FeatureFilterType> CurrentFilter = new ReactiveProperty<FeaturesFilter.FeatureFilterType>();

	public static FeaturesFilter.FeatureFilterType ThisSessionFilter;

	public FeaturesFilterVM()
	{
		CurrentFilter.Value = ThisSessionFilter;
	}

	protected override void DisposeImplementation()
	{
	}

	public void SetCurrentFilter(FeaturesFilter.FeatureFilterType filterData)
	{
		CurrentFilter.Value = filterData;
		ThisSessionFilter = filterData;
	}
}
