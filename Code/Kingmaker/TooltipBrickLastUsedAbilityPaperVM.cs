using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker;

public class TooltipBrickLastUsedAbilityPaperVM : TooltipBaseBrickVM
{
	public readonly string AbilityName;

	public readonly Sprite Icon;

	public TooltipBrickLastUsedAbilityPaperVM(string abilityName, Sprite icon)
	{
		AbilityName = abilityName;
		Icon = icon;
	}
}
