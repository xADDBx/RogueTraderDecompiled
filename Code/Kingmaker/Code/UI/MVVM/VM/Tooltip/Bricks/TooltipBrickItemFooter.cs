using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickItemFooter : TooltipBrickDoubleText
{
	private readonly Sprite m_Icon;

	public TooltipBrickItemFooter(string leftLine, string rightLine, Sprite icon)
		: base(leftLine, rightLine)
	{
		m_Icon = icon;
	}

	public override TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickItemFooterVM(m_LeftLine, m_RightLine, m_Icon);
	}
}
