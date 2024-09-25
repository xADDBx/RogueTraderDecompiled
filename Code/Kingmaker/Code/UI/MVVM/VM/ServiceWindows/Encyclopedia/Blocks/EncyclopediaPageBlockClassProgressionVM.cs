using Kingmaker.Blueprints.Encyclopedia;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia.Blocks;

public class EncyclopediaPageBlockClassProgressionVM : EncyclopediaPageBlockVM
{
	public readonly string Description;

	public EncyclopediaPageBlockClassProgressionVM(BlueprintEncyclopediaSkillPage.SkillTable block)
		: base(block)
	{
		Description = ((block.CharacterArchetype != null) ? block.CharacterArchetype.Description : block.CharacterClass.Description);
	}
}
