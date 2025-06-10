using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker;

public class TooltipBrickLastUsedAbilityPaper : ITooltipBrick
{
	private string m_AbilityName;

	private Sprite m_Icon;

	public TooltipBrickLastUsedAbilityPaper(string abilityName, Sprite icon)
	{
		m_AbilityName = abilityName;
		m_Icon = icon;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickLastUsedAbilityPaperVM(m_AbilityName, m_Icon);
	}
}
