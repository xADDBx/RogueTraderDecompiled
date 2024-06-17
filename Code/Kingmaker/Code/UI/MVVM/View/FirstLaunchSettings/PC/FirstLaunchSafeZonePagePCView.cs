using Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Base;
using Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.PC;

public class FirstLaunchSafeZonePagePCView : FirstLaunchSettingsPageBaseView<FirstLaunchSafeZonePageVM>
{
	[SerializeField]
	private SettingsEntitySliderPCView m_OffsetSlider;

	[SerializeField]
	private RectTransform m_Frame;

	private int ScreenWidth => Screen.width;

	private int ScreenHeight => Screen.height;

	protected override void BindViewImplementation()
	{
		m_OffsetSlider.Bind(base.ViewModel.Offset);
		AddDisposable(base.ViewModel.Offset.TempFloatValue.Subscribe(delegate(float value)
		{
			float num = value / 200f;
			m_Frame.offsetMin = new Vector2((float)ScreenWidth * num, (float)ScreenHeight * num);
			m_Frame.offsetMax = new Vector2((float)(-ScreenWidth) * num, (float)(-ScreenWidth) * num);
		}));
		base.BindViewImplementation();
	}

	protected override void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		navigationBehaviour.SetEntitiesVertical<SettingsEntitySliderPCView>(m_OffsetSlider);
		navigationBehaviour.AddRow(AdditionalEntities);
		navigationBehaviour.FocusOnFirstValidEntity();
	}
}
