using System.Collections.Generic;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;

public interface ICharInfoIgnoreNavigationConsoleView
{
	List<GridConsoleNavigationBehaviour> GetIgnoreNavigation();
}
