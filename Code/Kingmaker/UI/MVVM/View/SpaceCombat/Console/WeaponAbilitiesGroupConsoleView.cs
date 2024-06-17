using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.MVVM.View.SpaceCombat.Base;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.UI.MVVM.View.SpaceCombat.Console;

public class WeaponAbilitiesGroupConsoleView : WeaponAbilitiesGroupBaseView
{
	public List<IConsoleNavigationEntity> GetConsoleEntities()
	{
		return SlotsList.Cast<IConsoleNavigationEntity>().ToList();
	}
}
