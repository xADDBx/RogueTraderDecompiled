using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.Code.UI.MVVM.View.Vendor.Console;

public class VendorReputationForItemWindowConsoleView : VendorReputationForItemWindowView
{
	private GridConsoleNavigationBehaviour NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(NavigationBehaviour = new GridConsoleNavigationBehaviour());
	}

	public void ForceScrollToTop()
	{
		m_VirtualList.ScrollController.ForceScrollToTop();
	}

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		if (NavigationBehaviour == null)
		{
			AddDisposable(NavigationBehaviour = new GridConsoleNavigationBehaviour());
		}
		else
		{
			NavigationBehaviour.Clear();
		}
		NavigationBehaviour.AddEntityGrid(m_VirtualList.GetNavigationBehaviour());
		return NavigationBehaviour;
	}
}
