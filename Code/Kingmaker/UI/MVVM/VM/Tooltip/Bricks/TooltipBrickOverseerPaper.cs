using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickOverseerPaper : ITooltipBrick
{
	private readonly Sprite m_Icon;

	private readonly string m_Name;

	private readonly string m_Title;

	private readonly BaseUnitEntity UnitToShow;

	public TooltipBrickOverseerPaper(Sprite icon, string name, string title, BaseUnitEntity unitToShow = null)
	{
		m_Icon = icon;
		m_Name = name;
		m_Title = title;
		UnitToShow = unitToShow;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickOverseerPaperVM(m_Icon, m_Name, m_Title, UnitToShow);
	}
}
