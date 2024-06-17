using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickIconAndTextWithCustomColors : ITooltipBrick
{
	private readonly string m_StringValue;

	private readonly Sprite m_Icon;

	private readonly Color m_StringValueColor;

	private readonly Color m_IconColor;

	private readonly Color m_BackgroundColor;

	public TooltipBrickIconAndTextWithCustomColors(string stringValue, Sprite icon, Color stringValueColor, Color iconColor, Color backgroundColor)
	{
		m_StringValue = stringValue;
		m_Icon = icon;
		m_StringValueColor = stringValueColor;
		m_IconColor = iconColor;
		m_BackgroundColor = backgroundColor;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickIconAndTextWithCustomColorsVM(m_StringValue, m_Icon, m_StringValueColor, m_IconColor, m_BackgroundColor);
	}
}
