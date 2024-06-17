using Kingmaker.Blueprints.Encyclopedia.Blocks;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia.Blocks;

public class EncyclopediaPageBlockTextVM : EncyclopediaPageBlockVM
{
	public string Text => (m_Block as BlueprintEncyclopediaBlockText)?.GetText();

	public EncyclopediaPageBlockTextVM(BlueprintEncyclopediaBlockText block)
		: base(block)
	{
	}
}
