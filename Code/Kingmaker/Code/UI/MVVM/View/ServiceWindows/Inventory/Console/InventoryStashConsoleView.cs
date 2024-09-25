using Kingmaker.Code.UI.MVVM.View.Slots;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;

public class InventoryStashConsoleView : InventoryStashView
{
	public ItemsFilterConsoleView ItemsFilter => m_ItemsFilter as ItemsFilterConsoleView;

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		if (!base.IsBinded)
		{
			return null;
		}
		if (!m_ItemSlotsGroup.IsBinded)
		{
			return m_InsertableSlotsGroup.VirtualList.GetNavigationBehaviour();
		}
		return m_ItemSlotsGroup.VirtualList.GetNavigationBehaviour();
	}

	public IConsoleEntity GetCurrentFocus()
	{
		return GetNavigation().DeepestNestedFocus;
	}
}
