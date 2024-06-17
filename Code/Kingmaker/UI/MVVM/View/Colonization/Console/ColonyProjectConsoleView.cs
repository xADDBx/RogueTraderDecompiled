using Kingmaker.UI.MVVM.View.Colonization.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.UI.MVVM.View.Colonization.Console;

public class ColonyProjectConsoleView : ColonyProjectBaseView, IConsoleNavigationEntity, IConsoleEntity
{
	public void SetFocus(bool value)
	{
		m_MultiButton.SetFocus(value);
		if (value)
		{
			SelectPage();
		}
	}

	public bool IsValid()
	{
		return base.isActiveAndEnabled;
	}
}
