using Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Base;
using Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.PC.Entities;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.PC;

public class FirstLaunchLanguagePagePCView : FirstLaunchSettingsPageBaseView<FirstLaunchLanguagePageVM>
{
	[SerializeField]
	private FirstLaunchEntityLanguagePCView m_FirstLaunchEntityLanguagePCView;

	protected override void SetNavigationBehaviourImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		m_FirstLaunchEntityLanguagePCView.SetNavigationBehaviour(navigationBehaviour);
	}

	protected override void BindViewImplementation()
	{
		m_FirstLaunchEntityLanguagePCView.Bind(base.ViewModel.Languages);
		base.BindViewImplementation();
	}

	protected override void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		navigationBehaviour.AddRow(AdditionalEntities);
	}
}
