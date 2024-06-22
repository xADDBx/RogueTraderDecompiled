using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console.CareerPathProgression.SelectionTabs;

public class CareerPathSelectionsSummaryConsoleView : BaseCareerPathSelectionTabConsoleView<CareerPathVM>
{
	[Header("Info View")]
	[SerializeField]
	private InfoSectionView m_InfoView;

	public override void Initialize()
	{
		base.Initialize();
		m_InfoView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetHeader(UIStrings.Instance.CharacterSheet.HeaderSummaryTab);
		m_InfoView.Bind(base.ViewModel.TabInfoSectionVM);
	}

	public override void UpdateState()
	{
	}

	protected override void HandleClickNext()
	{
		if (base.IsBinded)
		{
			base.ViewModel.Commit();
		}
	}

	protected override void HandleClickBack()
	{
		base.ViewModel.SelectPreviousItem();
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		return m_InfoView.GetNavigationBehaviour();
	}
}
