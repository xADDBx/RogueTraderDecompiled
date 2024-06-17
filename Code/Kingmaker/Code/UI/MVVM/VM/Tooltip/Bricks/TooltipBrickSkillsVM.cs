using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Skills;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickSkillsVM : TooltipBaseBrickVM
{
	public CharInfoSkillsBlockVM AbilityScoresBlock;

	public TooltipBrickSkillsVM(CharInfoSkillsBlockVM abilityScoresBlock)
	{
		AddDisposable(AbilityScoresBlock = abilityScoresBlock);
	}
}
