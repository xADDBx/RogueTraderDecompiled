using Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Base;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.View.Settings.Console.Entities;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Console;

public class FirstLaunchAccessibilityPageConsoleView : FirstLaunchSettingsPageBaseView<FirstLaunchAccessiabilityPageVM>
{
	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	private SettingsEntitySliderConsoleView m_SettingsEntitySliderProtanopiaConsoleView;

	[SerializeField]
	private SettingsEntitySliderConsoleView m_SettingsEntitySliderDeuteranopiaConsoleView;

	[SerializeField]
	private SettingsEntitySliderConsoleView m_SettingsEntitySliderTritanopiaConsoleView;

	protected override void InitializeImpl()
	{
	}

	protected override void BindViewImplementation()
	{
		m_SettingsEntitySliderProtanopiaConsoleView.Bind(base.ViewModel.Protanopia);
		m_SettingsEntitySliderDeuteranopiaConsoleView.Bind(base.ViewModel.Deuteranopia);
		m_SettingsEntitySliderTritanopiaConsoleView.Bind(base.ViewModel.Tritanopia);
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
		navigationBehaviour.SetEntitiesVertical<SettingsEntitySliderConsoleView>(m_SettingsEntitySliderProtanopiaConsoleView, m_SettingsEntitySliderDeuteranopiaConsoleView, m_SettingsEntitySliderTritanopiaConsoleView);
		navigationBehaviour.FocusOnFirstValidEntity();
	}
}
