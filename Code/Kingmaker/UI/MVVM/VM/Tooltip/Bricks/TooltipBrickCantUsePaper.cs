using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickCantUsePaper : ITooltipBrick
{
	private string m_AbilityName;

	private Sprite m_Icon;

	private string m_CantUseTitle;

	public TooltipBrickCantUsePaper(string cantUseTitle, string abilityName, Sprite icon)
	{
		m_AbilityName = abilityName;
		m_Icon = icon;
		m_CantUseTitle = cantUseTitle;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickCantUsePaperVM(m_CantUseTitle, m_AbilityName, m_Icon);
	}
}
