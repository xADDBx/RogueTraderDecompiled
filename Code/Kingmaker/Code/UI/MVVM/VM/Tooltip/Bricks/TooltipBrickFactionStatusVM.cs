using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickFactionStatusVM : TooltipBaseBrickVM
{
	public readonly Sprite Icon;

	public readonly string Label;

	public readonly string Status;

	public TooltipBrickFactionStatusVM(Sprite icon, string label, string status)
	{
		Icon = icon;
		Label = label;
		Status = status;
	}
}
