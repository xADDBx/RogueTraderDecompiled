using System.Linq;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.FactionsReputation;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Utility;

namespace Kingmaker.Code.UI.MVVM.View.Vendor.Console;

public class VendorInfoFactionReputationItemConsoleView : CharInfoFactionReputationItemConsoleView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public bool HasVendors
	{
		get
		{
			if (m_WidgetList != null)
			{
				return m_WidgetList.VisibleEntries.Count > 0;
			}
			return false;
		}
	}

	public GridConsoleNavigationBehaviour GetNavigation()
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
		gridConsoleNavigationBehaviour.AddEntityHorizontal(this);
		m_NavigationBehaviour.AddEntityGrid(gridConsoleNavigationBehaviour);
		return m_NavigationBehaviour;
	}

	public void TryTrade()
	{
		if (HasVendors && m_WidgetList.VisibleEntries.FirstOrDefault((IWidgetView x) => x as FactionVendorInformationBaseView) is FactionVendorInformationBaseView factionVendorInformationBaseView)
		{
			factionVendorInformationBaseView.StartTrade();
		}
	}
}
