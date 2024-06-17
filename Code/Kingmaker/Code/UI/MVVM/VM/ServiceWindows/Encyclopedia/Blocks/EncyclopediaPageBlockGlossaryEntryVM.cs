using Kingmaker.Blueprints.Encyclopedia;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia.Blocks;

public class EncyclopediaPageBlockGlossaryEntryVM : EncyclopediaPageBlockVM
{
	public readonly string Title;

	public readonly string Description;

	public readonly bool Marked;

	public EncyclopediaPageBlockGlossaryEntryVM(GlossaryEntryBlock block, bool marked = false)
		: base(block)
	{
		Title = block.Entry.GetTitle();
		Description = block.Entry.GetDescription();
		Marked = marked;
	}

	protected override void DisposeImplementation()
	{
	}
}
