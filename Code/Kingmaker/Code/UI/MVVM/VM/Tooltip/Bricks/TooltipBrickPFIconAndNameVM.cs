using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickPFIconAndNameVM : TooltipBaseBrickVM
{
	public readonly Sprite Icon;

	public readonly string Label;

	public TooltipBrickPFIconAndNameVM(Sprite icon, string label)
	{
		Icon = icon;
		Label = label;
	}
}
