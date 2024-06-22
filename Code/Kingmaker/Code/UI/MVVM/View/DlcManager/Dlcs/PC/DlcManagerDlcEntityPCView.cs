using Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.Base;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.PC;

public class DlcManagerDlcEntityPCView : DlcManagerDlcEntityBaseView
{
	protected override void OnChangeSelectedStateImpl(bool value)
	{
		base.OnChangeSelectedStateImpl(value);
		SetFocus(value);
	}
}
