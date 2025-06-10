using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Utility;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickPetInfoConsoleView : TooltipBrickPetInfoView, IConsoleTooltipBrick
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateNavigationBehaviour();
	}

	public IConsoleEntity GetConsoleEntity()
	{
		return m_NavigationBehaviour;
	}

	private void CreateNavigationBehaviour()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
		List<TooltipBrickIconAndNameConsoleView> list = m_AbilitiesList.Entries.Select((IWidgetView e) => e as TooltipBrickIconAndNameConsoleView).ToList();
		for (int i = 0; i < list.Count / 2; i++)
		{
			if (i * 2 + 1 < list.Count)
			{
				m_NavigationBehaviour.AddRow<IConsoleEntity>(list[i * 2].GetConsoleEntity(), list[i * 2 + 1].GetConsoleEntity());
			}
			else
			{
				m_NavigationBehaviour.AddRow<IConsoleEntity>(list[i * 2].GetConsoleEntity());
			}
		}
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
