using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.FactionsReputation;

public class FactionVendorInformationConsoleView : FactionVendorInformationBaseView, IConfirmClickHandler, IConsoleEntity
{
	public bool CanConfirmClick()
	{
		return base.ViewModel.Vendor != null;
	}

	public void OnConfirmClick()
	{
		base.ViewModel.StartTrade();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
