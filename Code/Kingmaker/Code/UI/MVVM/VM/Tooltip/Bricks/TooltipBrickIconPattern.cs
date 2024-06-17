using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks.Utils;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickIconPattern : ITooltipBrick
{
	public class TextFieldValues
	{
		public string Text;

		public string Value;

		public TextFieldParams TextParams;

		public TextFieldParams ValueParams;
	}

	private readonly Sprite m_Icon;

	private readonly Color32? m_FrameColor;

	private readonly UIUtilityItem.UIPatternData m_PatternData;

	private readonly TextFieldValues m_TitleValues;

	private readonly TextFieldValues m_SecondaryValues;

	private readonly TextFieldValues m_TertiaryValues;

	private readonly TooltipBaseTemplate m_Tooltip;

	public TooltipBrickIconPattern(Sprite icon, UIUtilityItem.UIPatternData patternData, TextFieldValues titleValues, TextFieldValues secondaryValues = null, TextFieldValues tertiaryValues = null, Color32? frameColor = null, TooltipBaseTemplate tooltip = null)
	{
		m_Icon = icon;
		m_PatternData = patternData;
		m_TitleValues = titleValues;
		m_SecondaryValues = secondaryValues;
		m_TertiaryValues = tertiaryValues;
		m_FrameColor = frameColor;
		m_Tooltip = tooltip;
	}

	public TooltipBrickIconPattern(Sprite icon, UIUtilityItem.UIPatternData patternData, string title, string secondary = null, string tertiary = null, Color32? frameColor = null, TooltipBaseTemplate tooltip = null)
		: this(icon, patternData, new TextFieldValues
		{
			Text = title
		}, new TextFieldValues
		{
			Text = secondary
		}, new TextFieldValues
		{
			Text = tertiary
		}, frameColor, tooltip)
	{
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickIconPatternVM(m_Icon, m_PatternData, m_TitleValues, m_SecondaryValues, m_TertiaryValues, m_FrameColor, m_Tooltip);
	}
}
