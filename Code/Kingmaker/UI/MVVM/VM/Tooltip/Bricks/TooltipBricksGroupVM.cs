using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBricksGroupVM : TooltipBaseBrickVM
{
	public readonly TooltipBricksGroupType Type;

	public readonly bool HasBackground;

	public readonly Color? BackgroundColor;

	public readonly TooltipBricksGroupLayoutParams LayoutParams;

	public TooltipBricksGroupVM(TooltipBricksGroupType type, bool hasBackground, TooltipBricksGroupLayoutParams layoutParams, Color? backgroundColor)
	{
		Type = type;
		HasBackground = hasBackground;
		LayoutParams = layoutParams;
		BackgroundColor = backgroundColor;
	}
}
