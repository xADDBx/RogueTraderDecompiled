using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.Space.PC;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.UI.MVVM.View.Space.Console;

public class ShipStatsConsoleView : ShipStatsPCView
{
	public List<IFloatConsoleNavigationEntity> GetNavigation(List<IFloatConsoleNavigationEntity> additionalEntities)
	{
		List<IFloatConsoleNavigationEntity> list = new List<IFloatConsoleNavigationEntity>();
		list.Add(new SimpleConsoleNavigationEntity(m_SpeedBlock, m_SpeedTooltip, null, TooltipPlace));
		list.Add(new SimpleConsoleNavigationEntity(m_InertiaBlock, m_InertiaTooltip, null, TooltipPlace));
		list.AddRange(additionalEntities);
		return list;
	}
}
