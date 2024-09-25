using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickTitle : ITooltipBrick
{
	private readonly string m_Title;

	private readonly TooltipTitleType m_Type;

	private readonly TextAlignmentOptions m_Alignment;

	private readonly TextAnchor m_TextAnchor;

	private readonly int m_AdditionalTextSize;

	public TooltipBrickTitle(string title, TooltipTitleType type, TextAlignmentOptions alignment = TextAlignmentOptions.Center, TextAnchor textAnchor = TextAnchor.MiddleCenter, int additionalTextSize = 0)
	{
		m_Title = title;
		m_Type = type;
		m_Alignment = alignment;
		m_TextAnchor = textAnchor;
		m_AdditionalTextSize = additionalTextSize;
	}

	public TooltipBrickTitle(string title)
	{
		m_Title = title;
		m_Type = TooltipTitleType.H1;
		m_Alignment = TextAlignmentOptions.Center;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickTitleVM(m_Title, m_Type, m_Alignment, m_TextAnchor, m_AdditionalTextSize);
	}
}
