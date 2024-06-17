using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickCalculatedFormulaConsoleView : TooltipBrickCalculatedFormulaView, IConsoleTooltipBrick, IConsoleNavigationEntity, IConsoleEntity, IMonoBehaviour
{
	[Header("Console")]
	[SerializeField]
	protected OwlcatMultiButton m_MultiButton;

	public MonoBehaviour MonoBehaviour => this;

	public IConsoleEntity GetConsoleEntity()
	{
		return this;
	}

	public void SetFocus(bool value)
	{
		m_MultiButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_MultiButton.IsValid();
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
