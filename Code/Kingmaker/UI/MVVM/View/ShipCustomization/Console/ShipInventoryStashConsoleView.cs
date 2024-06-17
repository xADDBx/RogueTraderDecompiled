using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Space.PC;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.UI.MVVM.View.ShipCustomization.Console;

public class ShipInventoryStashConsoleView : ShipInventoryStashView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private SimpleConsoleNavigationEntity m_ScrapEntity;

	public ShipItemsFilterConsoleView ItemsFilter => m_ItemsFilter as ShipItemsFilterConsoleView;

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
		GridConsoleNavigationBehaviour navigationBehaviour = m_ItemSlotsGroup.VirtualList.GetNavigationBehaviour();
		m_NavigationBehaviour.AddEntityVertical(new SimpleConsoleNavigationEntity(m_CoinsContainer, new TooltipTemplateSimple(UIStrings.Instance.ShipCustomization.Scrap, UIStrings.Instance.ShipCustomization.ScrapDescription)));
		m_NavigationBehaviour.AddEntityVertical(navigationBehaviour);
		m_NavigationBehaviour.SetCurrentEntity(navigationBehaviour);
		return m_NavigationBehaviour;
	}
}
