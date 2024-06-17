using System.Collections.Generic;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickPortraitFeatures : ITooltipBrick
{
	private readonly string m_Name;

	private readonly string m_AvailableText;

	private readonly bool m_Available;

	private readonly Sprite m_Portrait;

	private readonly List<Ability> m_DesperateMeasureAbilities;

	private readonly List<Ability> m_HeroicActAbilities;

	public TooltipBrickPortraitFeatures(string name, bool available, string availableText, Sprite portrait, List<Ability> desperateMeasureAbilities, List<Ability> heroicActAbilities)
	{
		m_Name = name;
		m_AvailableText = availableText;
		m_Available = available;
		m_Portrait = portrait;
		m_DesperateMeasureAbilities = desperateMeasureAbilities;
		m_HeroicActAbilities = heroicActAbilities;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickPortraitFeaturesVM(m_Name, m_Available, m_AvailableText, m_Portrait, m_DesperateMeasureAbilities, m_HeroicActAbilities);
	}
}
