using Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickFeatureConsoleView : TooltipBrickFeatureView, IConsoleTooltipBrick, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate, IMonoBehaviour
{
	[SerializeField]
	protected OwlcatMultiButton m_MultiButton;

	public OwlcatMultiButton MultiButton => m_MultiButton;

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

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip;
	}

	bool IConsoleTooltipBrick.get_IsBinded()
	{
		return base.IsBinded;
	}
}
