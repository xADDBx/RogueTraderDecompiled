using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBricksGroupStart : ITooltipBrick
{
	private readonly TooltipBricksGroupLayoutParams m_LayoutParams;

	private readonly bool m_HasBackground;

	private readonly Color? m_BackgroundColor;

	public TooltipBricksGroupStart(bool hasBackground = true, TooltipBricksGroupLayoutParams layoutParams = null, Color? backgroundColor = null)
	{
		m_HasBackground = hasBackground;
		m_LayoutParams = layoutParams;
		m_BackgroundColor = backgroundColor;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBricksGroupVM(TooltipBricksGroupType.Start, m_HasBackground, m_LayoutParams, m_BackgroundColor);
	}
}
