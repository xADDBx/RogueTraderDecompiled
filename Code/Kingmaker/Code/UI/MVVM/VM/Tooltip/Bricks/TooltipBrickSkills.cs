using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Skills;
using Kingmaker.EntitySystem.Stats;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickSkills : ITooltipBrick
{
	private readonly CharInfoSkillsBlockVM m_Skills;

	public TooltipBrickSkills(StatsContainer stats)
	{
		m_Skills = new CharInfoSkillsBlockVM(stats);
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickSkillsVM(m_Skills);
	}
}
