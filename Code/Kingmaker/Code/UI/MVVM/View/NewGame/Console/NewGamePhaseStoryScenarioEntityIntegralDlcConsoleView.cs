using Kingmaker.Code.UI.MVVM.View.NewGame.Base;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Console;

public class NewGamePhaseStoryScenarioEntityIntegralDlcConsoleView : NewGamePhaseStoryScenarioEntityIntegralDlcBaseView
{
	protected override void OnChangeSelectedStateImpl()
	{
		base.OnChangeSelectedStateImpl();
		base.ViewModel.SelectMe();
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		base.ViewModel.SetSelected(value);
	}
}
