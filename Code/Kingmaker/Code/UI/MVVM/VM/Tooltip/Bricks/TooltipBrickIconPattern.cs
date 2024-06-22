using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Levelup.Selections;
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

	private readonly IconPatternMode m_IconMode;

	private readonly string m_Acronym;

	private readonly TalentIconInfo m_TalentIconsInfo;

	private readonly UIUtilityItem.UIPatternData m_PatternData;

	private readonly TextFieldValues m_TitleValues;

	private readonly TextFieldValues m_SecondaryValues;

	private readonly TextFieldValues m_TertiaryValues;

	private readonly TooltipBaseTemplate m_Tooltip;

	public TooltipBrickIconPattern(Sprite icon, UIUtilityItem.UIPatternData patternData, TextFieldValues titleValues, TextFieldValues secondaryValues = null, TextFieldValues tertiaryValues = null, TooltipBaseTemplate tooltip = null, IconPatternMode iconMode = IconPatternMode.SkillMode, string acronym = null, TalentIconInfo iconsInfo = null)
	{
		m_Icon = icon;
		m_PatternData = patternData;
		m_TitleValues = titleValues;
		m_SecondaryValues = secondaryValues;
		m_TertiaryValues = tertiaryValues;
		m_Tooltip = tooltip;
		m_IconMode = iconMode;
		m_Acronym = acronym;
		m_TalentIconsInfo = iconsInfo;
	}

	public TooltipBrickIconPattern(Sprite icon, UIUtilityItem.UIPatternData patternData, string title, string secondary = null, string tertiary = null, TooltipBaseTemplate tooltip = null, IconPatternMode iconMode = IconPatternMode.SkillMode)
		: this(icon, patternData, new TextFieldValues
		{
			Text = title
		}, new TextFieldValues
		{
			Text = secondary
		}, new TextFieldValues
		{
			Text = tertiary
		}, tooltip, iconMode)
	{
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickIconPatternVM(m_Icon, m_PatternData, m_TitleValues, m_SecondaryValues, m_TertiaryValues, m_Tooltip, m_IconMode, m_Acronym, m_TalentIconsInfo);
	}
}
