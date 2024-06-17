using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Utility;

namespace Kingmaker.Code.UI.MVVM.View.Vendor.Console;

public class VendorTradePartConsoleView : VendorTradePartView<ItemsFilterConsoleView, VendorLevelItemsConsoleView, VendorTransitionWindowConsoleView>
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
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour = new GridConsoleNavigationBehaviour();
		GridConsoleNavigationBehaviour gridConsoleNavigationBehaviour2 = new GridConsoleNavigationBehaviour();
		if (!base.ViewModel.NeedHidePfAndReputation)
		{
			gridConsoleNavigationBehaviour.SetEntitiesHorizontal<SimpleConsoleNavigationEntity>(new SimpleConsoleNavigationEntity(m_ProfitFactorBackground, m_ProfitFactorTooltip));
		}
		List<IWidgetView> entries = m_WidgetList.Entries;
		if (entries != null && entries.Count > 0)
		{
			gridConsoleNavigationBehaviour2.SetEntitiesVertical(m_WidgetList.Entries?.Select((IWidgetView e) => (e as VendorLevelItemsConsoleView)?.GetNavigation()).ToList() ?? throw new InvalidOperationException());
		}
		m_NavigationBehaviour.SetEntitiesHorizontal<GridConsoleNavigationBehaviour>(gridConsoleNavigationBehaviour, gridConsoleNavigationBehaviour2);
		m_NavigationBehaviour.FocusOnEntityManual(gridConsoleNavigationBehaviour2.Entities.Any((IConsoleEntity x) => x.IsValid()) ? gridConsoleNavigationBehaviour2 : gridConsoleNavigationBehaviour);
		return m_NavigationBehaviour;
	}
}
