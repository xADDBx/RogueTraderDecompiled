using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickIconAndName : ITooltipBrick
{
	protected readonly string m_Line;

	protected readonly Sprite m_Icon;

	public readonly TooltipBrickElementType m_Type;

	public readonly bool m_Frame;

	private readonly TooltipBaseTemplate m_Tooltip;

	public TooltipBrickIconAndName(Sprite icon, string line, TooltipBrickElementType type = TooltipBrickElementType.Medium, bool frame = true, TooltipBaseTemplate tooltip = null)
	{
		m_Line = line;
		m_Icon = icon;
		m_Type = type;
		m_Frame = frame;
		m_Tooltip = tooltip;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickIconAndNameVM(m_Icon, m_Line, m_Type, m_Frame, m_Tooltip);
	}
}
