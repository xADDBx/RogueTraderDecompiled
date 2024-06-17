using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickResourceIconAndNameVM : TooltipBaseBrickVM
{
	public readonly Sprite Icon;

	public readonly string Label;

	public TooltipBrickResourceIconAndNameVM(Sprite icon, string label)
	{
		Icon = icon;
		Label = label;
	}
}
