using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.Utility.UnitDescription;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickAbilityScores : ITooltipBrick
{
	private readonly CharInfoAbilityScoresBlockVM m_AbilityScoresBlock;

	public TooltipBrickAbilityScores(UnitDescription.StatsData statsData)
	{
		m_AbilityScoresBlock = new CharInfoAbilityScoresBlockVM(statsData);
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickAbilityScoresVM(m_AbilityScoresBlock);
	}
}
