using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;

public class RankEntryFeatureGroupVM : BaseFeatureGroupVM<BaseRankEntryFeatureVM>
{
	public BlueprintFeature Owner { get; }

	public RankEntryFeatureGroupVM([NotNull] List<BaseRankEntryFeatureVM> featuresListGroup, BlueprintFeature owner = null)
		: base(featuresListGroup, (string)null, (string)null)
	{
		Owner = owner;
	}

	public void UpdateState(LevelUpManager levelUpManager)
	{
		FeatureList.ForEach(delegate(BaseRankEntryFeatureVM vm)
		{
			vm.UpdateState(levelUpManager);
		});
	}

	public void UpdateReadOnlyState()
	{
		FeatureList.ForEach(delegate(BaseRankEntryFeatureVM vm)
		{
			(vm as RankEntrySelectionFeatureVM)?.UpdateStateForReadOnly();
		});
	}

	public virtual List<VirtualListElementVMBase> GetAll()
	{
		List<VirtualListElementVMBase> list = new List<VirtualListElementVMBase>();
		if (FeatureList.Empty())
		{
			return list;
		}
		List<BaseRankEntryFeatureVM> collection = FeatureList.ToList();
		list.AddRange(collection);
		return list;
	}

	public List<VirtualListElementVMBase> GetFiltered(FeaturesFilter.FeatureFilterType? filter)
	{
		if (!filter.HasValue || filter.GetValueOrDefault() == FeaturesFilter.FeatureFilterType.None)
		{
			return GetAll();
		}
		List<VirtualListElementVMBase> list = new List<VirtualListElementVMBase>();
		List<BaseRankEntryFeatureVM> list2 = FeatureList.Where((BaseRankEntryFeatureVM f) => f.Feature.MeetsFilter(filter.Value)).ToList();
		switch (filter.Value)
		{
		case FeaturesFilter.FeatureFilterType.RecommendedFilter:
			list2.RemoveAll((BaseRankEntryFeatureVM f) => !f.IsRecommended);
			break;
		case FeaturesFilter.FeatureFilterType.FavoritesFilter:
			list2.RemoveAll((BaseRankEntryFeatureVM f) => !f.IsFavorite);
			break;
		}
		list.AddRange(list2);
		return list;
	}
}
