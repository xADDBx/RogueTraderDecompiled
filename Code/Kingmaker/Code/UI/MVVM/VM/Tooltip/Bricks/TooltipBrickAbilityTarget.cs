using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickAbilityTarget : ITooltipBrick
{
	protected readonly string m_Label;

	protected readonly string m_Text;

	protected readonly Sprite m_Icon;

	public readonly TooltipBrickElementType m_Type;

	private readonly TooltipBaseTemplate m_Tooltip;

	public TooltipBrickAbilityTarget(Sprite icon, string label, string text, TooltipBrickElementType type = TooltipBrickElementType.Medium, TooltipBaseTemplate tooltip = null)
	{
		m_Label = label;
		m_Text = text;
		m_Icon = icon;
		m_Type = type;
		m_Tooltip = tooltip;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickAbilityTargetVM(m_Icon, m_Label, m_Text, m_Type, m_Tooltip);
	}
}
