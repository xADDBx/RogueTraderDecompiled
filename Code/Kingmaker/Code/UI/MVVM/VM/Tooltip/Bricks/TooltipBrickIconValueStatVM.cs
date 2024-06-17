using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickIconValueStatVM : TooltipBaseBrickVM
{
	public readonly string Name;

	public readonly string Value;

	public readonly Sprite Icon;

	public readonly TooltipIconValueStatType Type;

	public readonly bool IsIconWhite;

	public readonly bool NeedChangeSize;

	public readonly int TextSize;

	public readonly int ValueSize;

	public readonly bool NeedChangeColor;

	public readonly Color NameTextColor;

	public readonly Color ValueTextColor;

	public readonly bool UseSecondaryLabelColor;

	public TooltipBrickIconValueStatVM(string name, string value, Sprite icon, TooltipIconValueStatType type, bool isWhite = false, bool needChangeSize = false, int textSize = 18, int valueSize = 18, bool needChangeColor = false, Color nameTextColor = default(Color), Color valueTextColor = default(Color), bool useSecondaryLabelColor = false)
	{
		Type = type;
		Name = name;
		Value = value;
		Icon = icon;
		IsIconWhite = isWhite;
		NeedChangeSize = needChangeSize;
		TextSize = textSize;
		ValueSize = valueSize;
		NeedChangeColor = needChangeColor;
		NameTextColor = nameTextColor;
		ValueTextColor = valueTextColor;
		UseSecondaryLabelColor = useSecondaryLabelColor;
	}
}
