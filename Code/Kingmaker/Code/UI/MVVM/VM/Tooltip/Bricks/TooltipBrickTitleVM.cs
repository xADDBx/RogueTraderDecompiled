using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickTitleVM : TooltipBaseBrickVM
{
	public readonly string Title;

	public readonly TooltipTitleType Type;

	public readonly TextAlignmentOptions Alignment;

	public readonly TextAnchor TextAnchor;

	public readonly int AdditionalTextSize;

	public TooltipBrickTitleVM(string title, TooltipTitleType type, TextAlignmentOptions alignment, TextAnchor textAnchor, int additionalTextSize)
	{
		Title = title;
		Type = type;
		Alignment = alignment;
		TextAnchor = textAnchor;
		AdditionalTextSize = additionalTextSize;
	}
}
