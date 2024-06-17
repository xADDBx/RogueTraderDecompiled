using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickRankEntrySelection : ITooltipBrick
{
	private readonly RankEntrySelectionVM m_RankEntrySelectionVM;

	public TooltipBrickRankEntrySelection(RankEntrySelectionVM rankEntrySelectionVM)
	{
		m_RankEntrySelectionVM = rankEntrySelectionVM;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickRankEntrySelectionVM(m_RankEntrySelectionVM);
	}
}
