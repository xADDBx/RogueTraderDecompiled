using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.InfoWindow;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;

public class RankEntryFeatureItemVM : BaseRankEntryFeatureVM, IRankEntrySelectItem, IHasTooltipTemplates
{
	public readonly bool IsUltimate;

	private InfoSectionVM m_InfoVM;

	private readonly Action<IRankEntrySelectItem> m_SelectAction;

	public InfoSectionVM InfoVM => m_InfoVM ?? CreateInfoVM();

	public RankEntryFeatureItemVM(int rank, CareerPathVM careerPathVM, UIFeature uiFeature, Action<IRankEntrySelectItem> selectAction)
		: base(careerPathVM, uiFeature)
	{
		Rank = rank;
		m_SelectAction = selectAction;
		CareerPathUIMetaData careerPathUIMetaData = careerPathVM.CareerPathUIMetaData;
		IsUltimate = careerPathUIMetaData != null && careerPathUIMetaData.UltimateFeatures.Contains(base.Feature);
		AddDisposable(CareerPathVM.FeaturesToVisit.ObserveCountChanged().Subscribe(delegate
		{
			UpdateFeatureState();
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
		if (Rank < CareerPathVM.CurrentRank.Value)
		{
			value = RankFeatureState.Committed;
		}
		else if (Rank == CareerPathVM.CurrentRank.Value)
		{
			value = (CareerPathVM.CanCommit.Value ? RankFeatureState.Selected : RankFeatureState.Committed);
		}
		else if ((CareerPathVM.IsInLevelupProcess || (currentLevelupRange.Item1 == 1 && currentLevelupRange.Item2 == 1)) && currentLevelupRange.Item1 != -1 && Rank >= currentLevelupRange.Item1 && Rank <= currentLevelupRange.Item2)
		{
			value = (CareerPathVM.IsVisited(this) ? RankFeatureState.Selected : RankFeatureState.Selectable);
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

	public List<TooltipBaseTemplate> TooltipTemplates()
	{
		return new List<TooltipBaseTemplate>
		{
			base.Tooltip.Value,
			base.HintTooltip
		};
	}
}
