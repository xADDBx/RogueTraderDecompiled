using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.Code.UI.MVVM.View.Loot.Console;

public class InteractionSlotPartConsoleView : InteractionSlotPartView
{
	public GridConsoleNavigationBehaviour NavigationBehaviour;

	public ConsoleNavigationBehaviour GetNavigation()
	{
		if (NavigationBehaviour == null)
		{
			AddDisposable(NavigationBehaviour = new GridConsoleNavigationBehaviour());
		}
		else
		{
			NavigationBehaviour.Clear();
		}
		NavigationBehaviour.AddEntityGrid(m_SlotView as LootSlotConsoleView);
		return NavigationBehaviour;
	}

	public IConsoleEntity GetCurrentFocus()
	{
		return NavigationBehaviour.DeepestNestedFocus;
	}
}
