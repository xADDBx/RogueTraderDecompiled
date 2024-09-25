using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickItemFooterVM : TooltipBrickDoubleTextVM
{
	public readonly Sprite Icon;

	public TooltipBrickItemFooterVM(string leftLine, string rightLine, Sprite icon)
		: base(leftLine, rightLine, TextAnchor.MiddleLeft, TextAnchor.MiddleRight)
	{
		Icon = icon;
	}
}
