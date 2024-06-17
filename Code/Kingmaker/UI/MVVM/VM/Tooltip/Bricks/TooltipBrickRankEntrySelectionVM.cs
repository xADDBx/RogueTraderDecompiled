using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickRankEntrySelectionVM : TooltipBaseBrickVM
{
	public readonly RankEntrySelectionVM RankEntrySelectionVM;

	public TooltipBrickRankEntrySelectionVM(RankEntrySelectionVM rankEntrySelectionVM)
	{
		RankEntrySelectionVM = rankEntrySelectionVM;
	}
}
