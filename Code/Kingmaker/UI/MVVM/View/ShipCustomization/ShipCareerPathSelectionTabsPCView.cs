using System.Collections.Generic;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.SelectionTabs;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.PC.CareerPathProgression;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.PC.CareerPathProgression.SelectionTabs;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipCareerPathSelectionTabsPCView : CareerPathSelectionTabsCommonView
{
	[SerializeField]
	protected CareerPathSelectionsSummaryPCView m_CareerPathSelectionsSummaryPCView;

	[SerializeField]
	protected RankEntryFeatureDescriptionPCView m_RankEntryFeatureDescriptionPCView;

	[SerializeField]
	protected RankEntryFeatureSelectionPCView m_RankEntryFeatureSelectionPCView;

	private CareerButtonsBlock m_ButtonsBlock;

	public void SetButtonsBlock(CareerButtonsBlock buttonsBlock)
	{
		m_ButtonsBlock = buttonsBlock;
	}

	public override void Initialize()
	{
		Tabs = new List<ICareerPathSelectionTabView> { m_CareerPathSelectionsSummaryPCView, m_RankEntryFeatureDescriptionPCView, m_RankEntryFeatureSelectionPCView };
		Tabs.ForEach(delegate(ICareerPathSelectionTabView tab)
		{
			tab.Initialize();
			(tab as ICareerPathSelectionTabPCView)?.SetButtonsBlock(m_ButtonsBlock);
		});
	}

	protected override void BindViewImplementation()
	{
		base.ViewModel.SetRankEntry(null);
		base.BindViewImplementation();
	}

	protected override void SetNewTab(SelectionTab newTab, IRankEntrySelectItem currentItem)
	{
		switch (newTab)
		{
		case SelectionTab.Summary:
			m_CareerPathSelectionsSummaryPCView.Bind(base.ViewModel);
			break;
		case SelectionTab.FeatureDescription:
			m_RankEntryFeatureDescriptionPCView.Bind(currentItem as RankEntryFeatureItemVM);
			break;
		case SelectionTab.FeatureSelection:
			m_RankEntryFeatureSelectionPCView.Bind(currentItem as RankEntrySelectionVM);
			break;
		}
	}

	protected override SelectionTab GetActiveTab(IRankEntrySelectItem currentItem)
	{
		if (!(currentItem is RankEntryFeatureItemVM))
		{
			if (currentItem is RankEntrySelectionVM)
			{
				return SelectionTab.FeatureSelection;
			}
			return SelectionTab.Summary;
		}
		return SelectionTab.FeatureDescription;
	}
}
