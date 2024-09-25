using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickResourceInfoConsoleView : TooltipBrickResourceInfoView, IConsoleTooltipBrick
{
	[SerializeField]
	private OwlcatMultiButton m_MultiButton;

	public IConsoleEntity GetConsoleEntity()
	{
		return new SimpleConsoleNavigationEntity(m_MultiButton);
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
