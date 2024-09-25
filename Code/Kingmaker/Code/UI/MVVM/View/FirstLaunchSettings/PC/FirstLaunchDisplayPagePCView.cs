using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Base;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.PC;

public class FirstLaunchDisplayPagePCView : FirstLaunchSettingsPageBaseView<FirstLaunchDisplayPageVM>
{
	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	private SettingsEntitySliderGammaCorrectionPCView m_SettingsEntitySliderGammaCorrectionPCView;

	[SerializeField]
	private SettingsEntitySliderGammaCorrectionPCView m_SettingsEntitySliderBrightnessPCView;

	[SerializeField]
	private SettingsEntitySliderGammaCorrectionPCView m_SettingsEntitySliderContrastPCView;

	[SerializeField]
	private TextMeshProUGUI m_DisplayImageText_1;

	[SerializeField]
	private TextMeshProUGUI m_DisplayImageText_2;

	[SerializeField]
	private TextMeshProUGUI m_DisplayImageText_3;

	protected override void InitializeImpl()
	{
	}

	protected override void BindViewImplementation()
	{
		m_SettingsEntitySliderGammaCorrectionPCView.Bind(base.ViewModel.GammaCorrection);
		m_SettingsEntitySliderBrightnessPCView.Bind(base.ViewModel.Brightness);
		m_SettingsEntitySliderContrastPCView.Bind(base.ViewModel.Contrast);
		UITextSettingsUI settingsUI = UIStrings.Instance.SettingsUI;
		m_DisplayImageText_1.text = settingsUI.DisplayImageShadows;
		m_DisplayImageText_2.text = settingsUI.DisplayImageMidtones;
		m_DisplayImageText_3.text = settingsUI.DisplayImageBrights;
		m_InfoView.Bind(base.ViewModel.InfoVM);
		base.BindViewImplementation();
	}

	protected override void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		navigationBehaviour.SetEntitiesVertical<SettingsEntitySliderGammaCorrectionPCView>(m_SettingsEntitySliderGammaCorrectionPCView, m_SettingsEntitySliderBrightnessPCView, m_SettingsEntitySliderContrastPCView);
		navigationBehaviour.AddRow(AdditionalEntities);
		navigationBehaviour.FocusOnFirstValidEntity();
	}
}
