using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickCantUsePaperVM : TooltipBaseBrickVM
{
	public readonly string AbilityName;

	public readonly Sprite Icon;

	public readonly string CantUseTitle;

	public TooltipBrickCantUsePaperVM(string cantUseTitle, string abilityName, Sprite icon)
	{
		AbilityName = abilityName;
		Icon = icon;
		CantUseTitle = cantUseTitle;
	}
}
