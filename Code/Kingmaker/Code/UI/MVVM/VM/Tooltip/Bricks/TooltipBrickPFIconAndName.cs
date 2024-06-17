using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickPFIconAndName : ITooltipBrick
{
	private readonly Sprite m_Icon;

	private readonly string m_Label;

	public TooltipBrickPFIconAndName(Sprite icon, string label)
	{
		m_Icon = icon;
		m_Label = label;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickPFIconAndNameVM(m_Icon, m_Label);
	}
}
