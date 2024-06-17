using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickSkillsConsoleView : TooltipBrickSkillsView, IConsoleTooltipBrick
{
	public IConsoleEntity GetConsoleEntity()
	{
		if (m_AbilityScoresBlockView is CharInfoSkillsBlockConsoleView charInfoSkillsBlockConsoleView)
		{
			return charInfoSkillsBlockConsoleView.GetConsoleEntity();
		}
		return new GridConsoleNavigationBehaviour();
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
