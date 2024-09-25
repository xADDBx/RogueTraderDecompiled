using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Base;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.View.Settings.Console.Entities;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Console;

public class FirstLaunchDisplayPageConsoleView : FirstLaunchSettingsPageBaseView<FirstLaunchDisplayPageVM>
{
	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	private SettingsEntitySliderGammaCorrectionConsoleView m_SettingsEntitySliderGammaCorrectionConsoleView;

	[SerializeField]
	private SettingsEntitySliderGammaCorrectionConsoleView m_SettingsEntitySliderBrightnessConsoleView;

	[SerializeField]
	private SettingsEntitySliderGammaCorrectionConsoleView m_SettingsEntitySliderContrastConsoleView;

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
		m_SettingsEntitySliderGammaCorrectionConsoleView.Bind(base.ViewModel.GammaCorrection);
		m_SettingsEntitySliderBrightnessConsoleView.Bind(base.ViewModel.Brightness);
		m_SettingsEntitySliderContrastConsoleView.Bind(base.ViewModel.Contrast);
		UITextSettingsUI settingsUI = UIStrings.Instance.SettingsUI;
		m_DisplayImageText_1.text = settingsUI.DisplayImageShadows;
		m_DisplayImageText_2.text = settingsUI.DisplayImageMidtones;
		m_DisplayImageText_3.text = settingsUI.DisplayImageBrights;
		m_InfoView.Bind(base.ViewModel.InfoVM);
		base.BindViewImplementation();
	}

	public void ScrollDescription(InputActionEventData arg1, float x)
	{
		if (base.gameObject.activeInHierarchy)
		{
			Scroll(x);
		}
	}

	private void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_InfoView.ScrollRectExtended.scrollSensitivity);
		m_InfoView.ScrollRectExtended.OnSmoothlyScroll(pointerEventData);
	}

	protected override void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		navigationBehaviour.SetEntitiesVertical<SettingsEntitySliderGammaCorrectionConsoleView>(m_SettingsEntitySliderGammaCorrectionConsoleView, m_SettingsEntitySliderBrightnessConsoleView, m_SettingsEntitySliderContrastConsoleView);
		navigationBehaviour.FocusOnFirstValidEntity();
	}
}
