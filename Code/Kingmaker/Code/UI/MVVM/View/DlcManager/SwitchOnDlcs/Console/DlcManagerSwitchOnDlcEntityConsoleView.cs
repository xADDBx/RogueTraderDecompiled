using Kingmaker.Code.UI.MVVM.View.DlcManager.SwitchOnDlcs.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.SwitchOnDlcs.Console;

public class DlcManagerSwitchOnDlcEntityConsoleView : DlcManagerSwitchOnDlcEntityBaseView, INavigationHorizontalDirectionsHandler, INavigationLeftDirectionHandler, IConsoleEntity, INavigationRightDirectionHandler
{
	protected override void OnChangeSelectedStateImpl(bool value)
	{
		base.OnChangeSelectedStateImpl(value);
		base.ViewModel.SelectMe();
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		m_MultiButton.SetFocus(value);
		m_MultiButton.SetActiveLayer(value ? "Selected" : "Normal");
	}

	public override bool IsValid()
	{
		return base.gameObject.activeSelf;
	}

	public bool HandleLeft()
	{
		SwitchValue();
		return true;
	}

	public bool HandleRight()
	{
		SwitchValue();
		return true;
	}
}
