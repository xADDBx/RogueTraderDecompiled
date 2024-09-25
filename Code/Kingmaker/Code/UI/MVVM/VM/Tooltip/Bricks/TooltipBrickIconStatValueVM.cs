using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickIconStatValueVM : TooltipBaseBrickVM
{
	public readonly string Name;

	public readonly string Value;

	public readonly string AddValue;

	public readonly Sprite Icon;

	public readonly Color? IconColor;

	public readonly float? IconSize;

	public readonly string IconText;

	public readonly TooltipBrickIconStatValueType Type;

	public readonly TooltipBrickIconStatValueType BackgroundType;

	public readonly TooltipBrickIconStatValueStyle TextStyle;

	public readonly string ValueHint;

	public readonly bool HasValue;

	public readonly TooltipBaseTemplate Tooltip;

	public readonly ReactiveProperty<string> ReactiveValue;

	public readonly ReactiveProperty<string> ReactiveAddValue;

	public TooltipBrickIconStatValueVM(string name, string value, string addValue, Sprite icon, Color? iconColor, float? iconSize, string iconText, TooltipBrickIconStatValueType type, TooltipBrickIconStatValueStyle textStyle, TooltipBrickIconStatValueType backgroundType, string valueHint, bool hasValue, TooltipBaseTemplate tooltip, ReactiveProperty<string> reactiveValue, ReactiveProperty<string> reactiveAddValue)
	{
		Name = name;
		Value = value;
		AddValue = addValue;
		Icon = icon;
		IconColor = iconColor;
		IconSize = iconSize;
		IconText = iconText;
		Type = type;
		TextStyle = textStyle;
		BackgroundType = backgroundType;
		ValueHint = valueHint;
		HasValue = hasValue;
		Tooltip = tooltip;
		ReactiveValue = reactiveValue;
		ReactiveAddValue = reactiveAddValue;
	}
}
