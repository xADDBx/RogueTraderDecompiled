using Kingmaker.Blueprints.Encyclopedia;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickHistoryManagement : ITooltipBrick
{
	private readonly BlueprintEncyclopediaGlossaryEntry m_GlossaryEntry;

	public TooltipBrickHistoryManagement(BlueprintEncyclopediaGlossaryEntry glossaryEntry)
	{
		m_GlossaryEntry = glossaryEntry;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickHistoryManagementVM(m_GlossaryEntry);
	}
}
