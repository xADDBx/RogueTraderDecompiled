using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Console.Bricks;

public class TooltipBrickValueStatFormulaConsoleView : TooltipBrickValueStatFormulaView
{
	[SerializeField]
	private OwlcatMultiButton m_MultiButton;

	public OwlcatMultiButton MultiButton => m_MultiButton;
}
