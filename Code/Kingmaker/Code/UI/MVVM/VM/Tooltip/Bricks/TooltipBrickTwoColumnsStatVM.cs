using Kingmaker.UI.Models.Tooltip;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickTwoColumnsStatVM : TooltipBaseBrickVM
{
	public readonly string NameLeft;

	public readonly string NameRight;

	public readonly string ValueLeft;

	public readonly string ValueRight;

	public readonly Sprite IconLeft;

	public readonly Sprite IconRight;

	public readonly ComparisonResult ComparisonLeft;

	public readonly ComparisonResult ComparisonRight;

	public readonly bool HighlightLeft;

	public readonly bool HighlightRight;

	public readonly float NameSize;

	public readonly float ValueSize;

	public TooltipBrickTwoColumnsStatVM(string nameLeft, string nameRight, string valueLeft, string valueRight, Sprite iconLeft, Sprite iconRight, ComparisonResult comparisonLeft, ComparisonResult comparisonRight, bool highlightLeft = false, bool highlightRight = false, float nameSize = float.NaN, float valueSize = float.NaN)
	{
		NameLeft = nameLeft;
		NameRight = nameRight;
		ValueLeft = valueLeft;
		ValueRight = valueRight;
		IconLeft = iconLeft;
		IconRight = iconRight;
		ComparisonLeft = comparisonLeft;
		ComparisonRight = comparisonRight;
		HighlightLeft = highlightLeft;
		HighlightRight = highlightRight;
		NameSize = nameSize;
		ValueSize = valueSize;
	}
}
