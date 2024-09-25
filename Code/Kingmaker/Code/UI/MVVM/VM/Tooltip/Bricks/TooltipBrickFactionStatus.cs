using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickFactionStatus : ITooltipBrick
{
	protected readonly Sprite m_Icon;

	protected readonly string m_Label;

	protected readonly string m_Status;

	public TooltipBrickFactionStatus(Sprite icon, string label, string status)
	{
		m_Icon = icon;
		m_Label = label;
		m_Status = status;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickFactionStatusVM(m_Icon, m_Label, m_Status);
	}
}
