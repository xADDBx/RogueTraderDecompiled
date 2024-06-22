using Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.Base;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.Console;

public class DlcManagerDlcEntityConsoleView : DlcManagerDlcEntityBaseView
{
	protected override void OnChangeSelectedStateImpl(bool value)
	{
		base.OnChangeSelectedStateImpl(value);
		base.ViewModel.SelectMe();
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		base.ViewModel.SetSelected(value);
	}
}
