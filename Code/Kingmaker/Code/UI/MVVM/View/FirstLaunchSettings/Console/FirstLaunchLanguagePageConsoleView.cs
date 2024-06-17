using Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Base;
using Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Console.Entities;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Console;

public class FirstLaunchLanguagePageConsoleView : FirstLaunchSettingsPageBaseView<FirstLaunchLanguagePageVM>
{
	[SerializeField]
	private FirstLaunchEntityLanguageConsoleView m_FirstLaunchEntityLanguageConsoleView;

	protected override void SetNavigationBehaviourImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		m_FirstLaunchEntityLanguageConsoleView.SetNavigationBehaviour(navigationBehaviour);
	}

	protected override void BindViewImplementation()
	{
		m_FirstLaunchEntityLanguageConsoleView.Bind(base.ViewModel.Languages);
		base.BindViewImplementation();
	}
}
