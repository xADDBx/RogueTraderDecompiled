using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;

public class RankEntryDescriptionVM : VirtualListElementVMBase
{
	public readonly string Description;

	public RankEntryDescriptionVM(string description)
	{
		Description = description;
	}

	protected override void DisposeImplementation()
	{
	}
}
