using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickPortraitAndNameVM : TooltipBaseBrickVM
{
	public readonly Sprite Icon;

	public readonly string Line;

	public readonly TooltipBrickTitle BrickTitle;

	public readonly int Difficulty;

	public readonly bool IsUsedSubtypeIcon;

	public readonly bool IsEnemy;

	public readonly bool IsFriend;

	public TooltipBrickPortraitAndNameVM(Sprite icon, string line, TooltipBrickTitle brickTitle, int difficulty, bool isUsedSubtypeIcon, bool isEnemy, bool isFriend)
	{
		Icon = icon;
		Line = line;
		BrickTitle = brickTitle;
		Difficulty = difficulty;
		IsUsedSubtypeIcon = isUsedSubtypeIcon;
		IsEnemy = isEnemy;
		IsFriend = isFriend;
	}
}
