using System;
using System.Collections.Generic;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.InfoWindow;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;

public class RankEntryFeatureItemVM : BaseRankEntryFeatureVM, IRankEntrySelectItem, IHasTooltipTemplates
{
	private InfoSectionVM m_InfoVM;

	private readonly Action<IRankEntrySelectItem> m_SelectAction;

	public InfoSectionVM InfoVM => m_InfoVM ?? CreateInfoVM();

	public int EntryRank => Rank.GetValueOrDefault();

	public BoolReactiveProperty HasUnavailableFeatures { get; } = new BoolReactiveProperty();


	public RankEntryFeatureItemVM(int rank, CareerPathVM careerPathVM, UIFeature uiFeature, Action<IRankEntrySelectItem> selectAction)
		: base(careerPathVM, uiFeature)
	{
		Rank = rank;
		m_SelectAction = selectAction;
		AddDisposable(CareerPathVM.FeaturesToVisit.ObserveCountChanged().Subscribe(delegate
		{
			DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(UpdateFeatureState);
		}));
		UpdateFeatureState();
	}

	private InfoSectionVM CreateInfoVM()
	{
		AddDisposable(m_InfoVM = new InfoSectionVM());
		InfoVM.SetTemplate(base.Tooltip.Value);
		return m_InfoVM;
	}

	protected sealed override void UpdateFeatureState()
	{
		RankFeatureState value = RankFeatureState.NotActive;
		(int, int) currentLevelupRange = CareerPathVM.GetCurrentLevelupRange();
		if ((CareerPathVM.IsInLevelupProcess || (currentLevelupRange.Item1 == 1 && currentLevelupRange.Item2 == 1)) && currentLevelupRange.Item1 != -1 && Rank >= currentLevelupRange.Item1 && Rank <= currentLevelupRange.Item2)
		{
			value = (CareerPathVM.IsVisited(this) ? RankFeatureState.Selected : RankFeatureState.Selectable);
		}
		else if (Rank <= CareerPathVM.CurrentRank.Value)
		{
			value = RankFeatureState.Committed;
		}
		FeatureState.Value = value;
	}

	public override void Select()
	{
		m_SelectAction?.Invoke(this);
	}

	public FeatureGroup? GetFeatureGroup()
	{
		return null;
	}

	public override bool CanSelect()
	{
		(int, int) currentLevelupRange = CareerPathVM.GetCurrentLevelupRange();
		if (FeatureState.Value != RankFeatureState.NotActive && Rank >= currentLevelupRange.Item1 && Rank <= currentLevelupRange.Item2)
		{
			return CareerPathVM.IsInLevelupProcess;
		}
		return false;
	}

	public void UpdateFeatures()
	{
	}

	public void UpdateReadOnlyState()
	{
	}

	public void ToggleShowUnavailableFeatures()
	{
	}

	public bool ContainsFeature(string key)
	{
		return false;
	}

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return new List<TooltipBaseTemplate>
		{
			base.Tooltip.Value,
			base.HintTooltip
		};
	}
}
