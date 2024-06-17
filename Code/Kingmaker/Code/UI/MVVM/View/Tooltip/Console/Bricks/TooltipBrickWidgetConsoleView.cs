using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickWidgetConsoleView : TooltipBrickWidgetView, IConsoleTooltipBrick
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddColumn(m_WidgetList.GetNavigationEntities());
	}

	public IConsoleEntity GetConsoleEntity()
	{
		CreateNavigation();
		return m_NavigationBehaviour;
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
