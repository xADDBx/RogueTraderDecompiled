using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.MVVM.View.SpaceCombat.Base;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.Console;

public class AbilitiesGroupConsoleView : AbilitiesGroupBaseView
{
	public List<IConsoleNavigationEntity> GetConsoleEntities()
	{
		return m_AbilitySlots.Cast<IConsoleNavigationEntity>().ToList();
	}
}
