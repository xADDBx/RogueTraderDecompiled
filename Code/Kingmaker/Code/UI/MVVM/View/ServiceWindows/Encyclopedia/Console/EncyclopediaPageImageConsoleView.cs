using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Base;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Console;

public class EncyclopediaPageImageConsoleView : EncyclopediaPageImageBaseView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ZoomButton.gameObject.SetActive(value: false);
	}
}
