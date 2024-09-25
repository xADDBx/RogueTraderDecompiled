using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickIconAndTextWithCustomColorsVM : TooltipBaseBrickVM
{
	public readonly string StringValue;

	public readonly Sprite Icon;

	public readonly Color StringValueColor;

	public readonly Color IconColor;

	public readonly Color BackgroundColor;

	public TooltipBrickIconAndTextWithCustomColorsVM(string stringValue, Sprite icon, Color stringValueColor, Color iconColor, Color backgroundColor)
	{
		StringValue = stringValue;
		Icon = icon;
		StringValueColor = stringValueColor;
		IconColor = iconColor;
		BackgroundColor = backgroundColor;
	}
}
