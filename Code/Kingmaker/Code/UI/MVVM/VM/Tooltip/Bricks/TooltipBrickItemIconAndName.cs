using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickItemIconAndName : ITooltipBrick
{
	private readonly Sprite m_Icon;

	private readonly string m_Label;

	public TooltipBrickItemIconAndName(Sprite icon, string label)
	{
		m_Icon = icon;
		m_Label = label;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickItemIconAndNameVM(m_Icon, m_Label);
	}
}
