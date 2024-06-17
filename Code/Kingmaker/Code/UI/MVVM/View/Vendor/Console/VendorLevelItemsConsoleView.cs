using System.Linq;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.Code.UI.MVVM.View.Vendor.Console;

public class VendorLevelItemsConsoleView : VendorLevelItemsBaseView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public ConsoleNavigationBehaviour GetNavigation()
	{
		if (m_NavigationBehaviour == null)
		{
			AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		}
		else
		{
			m_NavigationBehaviour.Clear();
		}
		m_NavigationBehaviour.SetEntitiesGrid(m_WidgetList.Entries.Cast<VendorSlotConsoleView>().ToList(), 1);
		return m_NavigationBehaviour;
	}
}
