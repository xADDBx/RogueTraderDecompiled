using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickItemIconAndNameVM : TooltipBaseBrickVM
{
	public readonly Sprite Icon;

	public readonly string Label;

	public TooltipBrickItemIconAndNameVM(Sprite icon, string label)
	{
		Icon = icon;
		Label = label;
	}
}
