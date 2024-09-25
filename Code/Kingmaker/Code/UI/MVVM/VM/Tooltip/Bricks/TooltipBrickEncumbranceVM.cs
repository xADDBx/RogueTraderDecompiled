using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickEncumbranceVM : TooltipBaseBrickVM
{
	public EncumbranceVM EncumbranceVM;

	public TooltipBrickEncumbranceVM(EncumbranceHelper.CarryingCapacity capacity)
	{
		AddDisposable(EncumbranceVM = new EncumbranceVM(capacity));
	}
}
