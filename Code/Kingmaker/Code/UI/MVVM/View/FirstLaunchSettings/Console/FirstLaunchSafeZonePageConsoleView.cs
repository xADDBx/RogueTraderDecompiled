using Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Base;
using Kingmaker.Code.UI.MVVM.View.Settings.Console.Entities;
using Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings;
using Kingmaker.UI.Canvases;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Console;

public class FirstLaunchSafeZonePageConsoleView : FirstLaunchSettingsPageBaseView<FirstLaunchSafeZonePageVM>
{
	[SerializeField]
	private SettingsEntitySliderConsoleView m_OffsetSlider;

	[SerializeField]
	private RectTransform m_Frame;

	protected override void BindViewImplementation()
	{
		m_OffsetSlider.Bind(base.ViewModel.Offset);
		AddDisposable(base.ViewModel.Offset.TempFloatValue.Subscribe(delegate(float value)
		{
			float num = value / 200f;
			Rect rect = MainCanvas.Instance.RectTransform.rect;
			m_Frame.offsetMin = new Vector2(rect.width * num, rect.height * num);
			m_Frame.offsetMax = new Vector2((0f - rect.width) * num, (0f - rect.height) * num);
		}));
		base.BindViewImplementation();
	}

	protected override void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		navigationBehaviour.SetEntitiesVertical<SettingsEntitySliderConsoleView>(m_OffsetSlider);
		navigationBehaviour.FocusOnFirstValidEntity();
	}
}
