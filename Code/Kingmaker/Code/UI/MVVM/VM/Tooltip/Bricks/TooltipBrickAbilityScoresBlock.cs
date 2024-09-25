using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickAbilityScoresBlock : ITooltipBrick
{
	private readonly CharInfoAbilityScoresBlockVM m_AbilityScoresBlock;

	private readonly ReactiveCommand m_CharacteristicsChanged;

	public TooltipBrickAbilityScoresBlock(IReadOnlyReactiveProperty<BaseUnitEntity> unit)
	{
		m_AbilityScoresBlock = new CharInfoAbilityScoresBlockVM(unit);
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickAbilityScoresBlockVM(m_AbilityScoresBlock);
	}
}
