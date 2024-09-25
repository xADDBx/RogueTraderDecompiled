using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickHistoryManagementVM : TooltipBaseBrickVM
{
	public readonly string Title;

	public readonly BoolReactiveProperty PreviousButtonInteractable = new BoolReactiveProperty();

	public readonly BoolReactiveProperty NextButtonInteractable = new BoolReactiveProperty();

	public TooltipBrickHistoryManagementVM(BlueprintEncyclopediaGlossaryEntry glossaryEntry)
	{
		Title = glossaryEntry.Title;
		CheckDirectionButtons();
	}

	public void CheckDirectionButtons()
	{
		PreviousButtonInteractable.Value = TooltipHelper.HistoryPointer?.Previous != null;
		NextButtonInteractable.Value = TooltipHelper.HistoryPointer?.Next != null;
	}

	public void OnPreviousButtonClick(GridConsoleNavigationBehaviour ownerBehaviour)
	{
		TooltipHelper.GlossaryHistoryPrevious(ownerBehaviour);
	}

	public void OnNextButtonClick(GridConsoleNavigationBehaviour ownerBehaviour)
	{
		TooltipHelper.GlossaryHistoryNext(ownerBehaviour);
	}
}
