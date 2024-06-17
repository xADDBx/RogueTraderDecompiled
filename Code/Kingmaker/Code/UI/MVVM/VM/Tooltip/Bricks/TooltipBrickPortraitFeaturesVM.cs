using System.Collections.Generic;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickPortraitFeaturesVM : TooltipBaseBrickVM
{
	public readonly string Name;

	public readonly bool Available;

	public readonly string AvailableText;

	public readonly Sprite Portrait;

	public readonly List<Ability> DesperateMeasureAbilities;

	public readonly List<Ability> HeroicActAbilities;

	public TooltipBrickPortraitFeaturesVM(string name, bool available, string availableText, Sprite portrait, List<Ability> desperateMeasureAbilities, List<Ability> heroicActAbilities)
	{
		Name = name;
		Available = available;
		AvailableText = availableText;
		Portrait = portrait;
		DesperateMeasureAbilities = desperateMeasureAbilities;
		HeroicActAbilities = heroicActAbilities;
	}
}
