using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.Code.UI.MVVM.View.Loot.Console;

public class LootObjectConsoleView : LootObjectView, IConsoleEntityProxy, IConsoleEntity
{
	public GridConsoleNavigationBehaviour NavigationBehaviour;

	public IConsoleEntity ConsoleEntityProxy => NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		NavigationBehaviour = base.SlotsGroup.GetNavigation();
	}
}
