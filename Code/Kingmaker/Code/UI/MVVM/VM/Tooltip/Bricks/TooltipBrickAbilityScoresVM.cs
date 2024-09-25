using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickAbilityScoresVM : TooltipBaseBrickVM
{
	public CharInfoAbilityScoresBlockVM AbilityScoresBlock;

	public TooltipBrickAbilityScoresVM(CharInfoAbilityScoresBlockVM abilityScoresBlock)
	{
		AddDisposable(AbilityScoresBlock = abilityScoresBlock);
	}
}
