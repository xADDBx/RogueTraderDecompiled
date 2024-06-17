using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickIconPatternVM : TooltipBaseBrickVM
{
	public readonly Sprite Icon;

	public readonly Color32? FrameColor;

	public readonly UIUtilityItem.UIPatternData PatternData;

	public readonly TooltipBrickIconPattern.TextFieldValues TitleValues;

	public readonly TooltipBrickIconPattern.TextFieldValues SecondaryValues;

	public readonly TooltipBrickIconPattern.TextFieldValues TertiaryValues;

	public readonly TooltipBaseTemplate Tooltip;

	public TooltipBrickIconPatternVM(Sprite icon, UIUtilityItem.UIPatternData patternData, TooltipBrickIconPattern.TextFieldValues titleValues, TooltipBrickIconPattern.TextFieldValues secondaryValues, TooltipBrickIconPattern.TextFieldValues tertiaryValues, Color32? frameColor, TooltipBaseTemplate tooltip)
	{
		Icon = icon;
		PatternData = patternData;
		TitleValues = titleValues;
		SecondaryValues = secondaryValues;
		TertiaryValues = tertiaryValues;
		FrameColor = frameColor;
		Tooltip = tooltip;
	}
}
