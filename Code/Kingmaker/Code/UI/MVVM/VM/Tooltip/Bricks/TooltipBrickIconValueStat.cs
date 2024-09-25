using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickIconValueStat : ITooltipBrick
{
	private readonly string m_Name;

	private readonly string m_Value;

	private readonly Sprite m_Icon;

	private readonly TooltipIconValueStatType m_Type;

	private readonly bool m_IsIconWhite;

	private readonly bool m_NeedChangeSize;

	private readonly int m_TextSize;

	private readonly int m_ValueSize;

	private readonly bool m_NeedChangeColor;

	private readonly Color m_NameTextColor;

	private readonly Color m_ValueTextColor;

	private readonly bool m_UseSecondaryLabelColor;

	public TooltipBrickIconValueStat(string name, string value, Sprite icon = null, TooltipIconValueStatType type = TooltipIconValueStatType.Normal, bool isWhite = false, bool needChangeSize = false, int textSize = 18, int valueSize = 18, bool needChangeColor = false, Color nameTextColor = default(Color), Color valueTextColor = default(Color), bool useSecondaryLabelColor = false)
	{
		m_Name = name;
		m_Value = value;
		m_Icon = icon;
		m_Type = type;
		m_IsIconWhite = isWhite;
		m_NeedChangeSize = needChangeSize;
		m_TextSize = textSize;
		m_ValueSize = valueSize;
		m_NeedChangeColor = needChangeColor;
		m_NameTextColor = nameTextColor;
		m_ValueTextColor = valueTextColor;
		m_UseSecondaryLabelColor = useSecondaryLabelColor;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickIconValueStatVM(m_Name, m_Value, m_Icon, m_Type, m_IsIconWhite, m_NeedChangeSize, m_TextSize, m_ValueSize, m_NeedChangeColor, m_NameTextColor, m_ValueTextColor, m_UseSecondaryLabelColor);
	}
}
