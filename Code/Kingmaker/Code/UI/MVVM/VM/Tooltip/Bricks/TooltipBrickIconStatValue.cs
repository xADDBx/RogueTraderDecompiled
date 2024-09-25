using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickIconStatValue : ITooltipBrick
{
	private readonly string m_Name;

	private readonly string m_Value;

	private readonly string m_AddValue;

	private readonly Sprite m_Icon;

	private readonly Color? m_IconColor;

	private readonly float? m_IconSize;

	private readonly string m_IconText;

	private readonly TooltipBrickIconStatValueType m_Type;

	private readonly TooltipBrickIconStatValueType m_BackgroundType;

	private readonly TooltipBrickIconStatValueStyle m_Style;

	private readonly string m_ValueHint;

	private readonly bool m_HasValue;

	private readonly TooltipBaseTemplate m_Tooltip;

	private readonly ReactiveProperty<string> m_ReactiveValue;

	private readonly ReactiveProperty<string> m_ReactiveAddValue;

	public TooltipBrickIconStatValue(string name, string value, string addValue = null, Sprite icon = null, TooltipBrickIconStatValueType type = TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType backgroundType = TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle style = TooltipBrickIconStatValueStyle.Normal, string valueHint = null, TooltipBaseTemplate tooltip = null, ReactiveProperty<string> reactiveValue = null, ReactiveProperty<string> reactiveAddValue = null, Color? iconColor = null, float? iconSize = null, string iconText = null, bool hasValue = true)
	{
		m_Name = name;
		m_Value = value;
		m_AddValue = addValue;
		m_Icon = icon;
		m_IconColor = iconColor;
		m_IconSize = iconSize;
		m_IconText = iconText;
		m_Type = type;
		m_Style = style;
		m_BackgroundType = backgroundType;
		m_ValueHint = valueHint;
		m_HasValue = hasValue;
		m_Tooltip = tooltip;
		m_ReactiveValue = reactiveValue;
		m_ReactiveAddValue = reactiveAddValue;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickIconStatValueVM(m_Name, m_Value, m_AddValue, m_Icon, m_IconColor, m_IconSize, m_IconText, m_Type, m_Style, m_BackgroundType, m_ValueHint, m_HasValue, m_Tooltip, m_ReactiveValue, m_ReactiveAddValue);
	}
}
