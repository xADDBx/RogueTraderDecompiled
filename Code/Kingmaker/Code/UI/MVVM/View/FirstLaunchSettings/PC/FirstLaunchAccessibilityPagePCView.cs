using Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Base;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.PC;

public class FirstLaunchAccessibilityPagePCView : FirstLaunchSettingsPageBaseView<FirstLaunchAccessiabilityPageVM>
{
	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	private SettingsEntitySliderFontSizePCView m_SettingsEntitySliderFontSizePCView;

	[SerializeField]
	private SettingsEntitySliderPCView m_SettingsEntitySliderProtanopiaPCView;

	[SerializeField]
	private SettingsEntitySliderPCView m_SettingsEntitySliderDeuteranopiaPCView;

	[SerializeField]
	private SettingsEntitySliderPCView m_SettingsEntitySliderTritanopiaPCView;

	protected override void InitializeImpl()
	{
	}

	protected override void BindViewImplementation()
	{
		m_SettingsEntitySliderFontSizePCView.Bind(base.ViewModel.FontSize);
		m_SettingsEntitySliderProtanopiaPCView.Bind(base.ViewModel.Protanopia);
		m_SettingsEntitySliderDeuteranopiaPCView.Bind(base.ViewModel.Deuteranopia);
		m_SettingsEntitySliderTritanopiaPCView.Bind(base.ViewModel.Tritanopia);
		m_InfoView.Bind(base.ViewModel.InfoVM);
		base.BindViewImplementation();
	}

	protected override void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		navigationBehaviour.SetEntitiesVertical<SettingsEntitySliderPCView>(m_SettingsEntitySliderFontSizePCView, m_SettingsEntitySliderProtanopiaPCView, m_SettingsEntitySliderDeuteranopiaPCView, m_SettingsEntitySliderTritanopiaPCView);
		navigationBehaviour.AddRow(AdditionalEntities);
		navigationBehaviour.FocusOnFirstValidEntity();
	}
}
