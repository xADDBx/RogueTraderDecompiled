using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.SelectionTabs;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console.CareerPathProgression.SelectionTabs;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.Console;

public class ShipCareerPathSelectionTabsConsoleView : CareerPathSelectionTabsCommonView
{
	[SerializeField]
	protected CareerPathSelectionsSummaryConsoleView m_CareerPathSelectionsSummaryConsoleView;

	[SerializeField]
	protected RankEntryFeatureDescriptionConsoleView m_RankEntryFeatureDescriptionConsoleView;

	[SerializeField]
	protected RankEntryFeatureSelectionConsoleView m_RankEntryFeatureSelectionConsoleView;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private ConsoleHintsWidget m_HintsWidget;

	public override void Initialize()
	{
		Tabs = new List<ICareerPathSelectionTabView> { m_CareerPathSelectionsSummaryConsoleView, m_RankEntryFeatureDescriptionConsoleView, m_RankEntryFeatureSelectionConsoleView };
		Tabs.ForEach(delegate(ICareerPathSelectionTabView tab)
		{
			tab.Initialize();
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
		case SelectionTab.CareerPathDescription:
			m_CareerPathSelectionsSummaryConsoleView.Bind(base.ViewModel);
			break;
		case SelectionTab.FeatureDescription:
			m_RankEntryFeatureDescriptionConsoleView.Bind(currentItem as RankEntryFeatureItemVM);
			break;
		case SelectionTab.FeatureSelection:
			m_RankEntryFeatureSelectionConsoleView.Bind(currentItem as RankEntrySelectionVM);
			break;
		}
		UpdateNavigation();
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
		UpdateNavigation();
	}

	private void UpdateNavigation()
	{
		if (m_NavigationBehaviour != null)
		{
			m_NavigationBehaviour.Clear();
			GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = null;
			switch (SavedTab)
			{
			case SelectionTab.FeatureSelection:
				gridConsoleNavigationBehaviour = m_RankEntryFeatureSelectionConsoleView.GetNavigationBehaviour();
				break;
			}
			if (gridConsoleNavigationBehaviour != null)
			{
				m_NavigationBehaviour.AddColumn<GridConsoleNavigationBehaviour>(gridConsoleNavigationBehaviour);
			}
			AddInput(ref m_InputLayer, ref m_HintsWidget);
		}
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		if (m_NavigationBehaviour == null)
		{
			CreateNavigation();
		}
		return m_NavigationBehaviour;
	}

	public void AddInput(ref InputLayer inputLayer, ref ConsoleHintsWidget hintsWidget)
	{
		m_InputLayer = inputLayer;
		m_HintsWidget = hintsWidget;
		foreach (ICareerPathSelectionTabConsoleView item in from ICareerPathSelectionTabConsoleView i in Tabs
			where i.IsTabActive()
			select i)
		{
			item.AddInput(inputLayer, hintsWidget);
		}
	}
}
