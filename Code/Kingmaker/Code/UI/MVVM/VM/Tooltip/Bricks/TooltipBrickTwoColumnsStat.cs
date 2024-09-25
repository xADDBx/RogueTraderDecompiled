using Kingmaker.UI.Models.Tooltip;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickTwoColumnsStat : ITooltipBrick
{
	private readonly string m_NameLeft;

	private readonly string m_NameRight;

	private readonly string m_ValueLeft;

	private readonly string m_ValueRight;

	private readonly Sprite m_IconLeft;

	private readonly Sprite m_IconRight;

	private readonly ComparisonResult m_ComparisonLeft;

	private readonly ComparisonResult m_ComparisonRight;

	private readonly float m_NameSize;

	private readonly float m_ValueSize;

	private readonly bool m_HighlightLeft;

	private readonly bool m_HighlightRight;

	public TooltipBrickTwoColumnsStat(string nameLeft, string nameRight, string valueLeft, string valueRight, Sprite iconLeft = null, Sprite iconRight = null, ComparisonResult comparisonLeft = ComparisonResult.Equal, ComparisonResult comparisonRight = ComparisonResult.Equal, bool highlightLeft = false, bool highlightRight = false, float nameSize = float.NaN, float valueSize = float.NaN)
	{
		m_NameLeft = nameLeft;
		m_NameRight = nameRight;
		m_ValueLeft = valueLeft;
		m_ValueRight = valueRight;
		m_IconLeft = iconLeft;
		m_IconRight = iconRight;
		m_ComparisonLeft = comparisonLeft;
		m_ComparisonRight = comparisonRight;
		m_NameSize = nameSize;
		m_ValueSize = valueSize;
		m_HighlightLeft = highlightLeft;
		m_HighlightRight = highlightRight;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickTwoColumnsStatVM(m_NameLeft, m_NameRight, m_ValueLeft, m_ValueRight, m_IconLeft, m_IconRight, m_ComparisonLeft, m_ComparisonRight, m_HighlightLeft, m_HighlightRight, m_NameSize, m_ValueSize);
	}
}
