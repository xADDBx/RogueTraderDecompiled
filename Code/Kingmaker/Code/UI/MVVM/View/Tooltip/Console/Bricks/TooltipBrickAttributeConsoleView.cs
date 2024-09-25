using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickAttributeConsoleView : TooltipBrickAttributeView, IConsoleTooltipBrick
{
	[Header("Console")]
	[SerializeField]
	private OwlcatMultiButton m_MultiButton;

	public IConsoleEntity GetConsoleEntity()
	{
		return new SimpleConsoleNavigationEntity(m_MultiButton, base.ViewModel.Tooltip);
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
