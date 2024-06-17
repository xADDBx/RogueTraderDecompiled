using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickAbilityScoresBlockVM : TooltipBaseBrickVM
{
	public CharInfoAbilityScoresBlockVM AbilityScoresBlock;

	public TooltipBrickAbilityScoresBlockVM(CharInfoAbilityScoresBlockVM abilityScoresBlock)
	{
		AddDisposable(AbilityScoresBlock = abilityScoresBlock);
	}
}
